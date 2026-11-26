using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class PostingAnalysisService(ApplicationDbContext context, ILlmService llm, TimeProvider clock,
    ILogger<PostingAnalysisService> logger)
{
    internal async Task<PostingAnalysis> QueueAsync(InternshipPosting posting, bool retryFailed, CancellationToken cancellationToken)
    {
        var fingerprint = PostingAnalysisSource.From(posting).Fingerprint();
        var latest = await LatestAsync(posting.Id, cancellationToken);
        if (IsCurrent(latest, fingerprint) &&
            (latest!.Status is AnalysisStatus.Pending or AnalysisStatus.Processing ||
                latest.SchemaVersion == PostingAnalysisContract.SchemaVersion && (!retryFailed || latest.Status != AnalysisStatus.Failed)))
            return latest!;

        var pending = new PostingAnalysis
        {
            InternshipPostingId = posting.Id,
            SourceRowVersion = posting.RowVersion,
            SourceContentHash = fingerprint,
            Version = (latest?.Version ?? 0) + 1,
            SchemaVersion = PostingAnalysisContract.SchemaVersion,
            CreatedAt = clock.GetUtcNow()
        };
        context.PostingAnalyses.Add(pending);
        return pending;
    }

    public async Task<PostingAnalysisDto?> GetAsync(string userId, int postingId, CancellationToken cancellationToken)
    {
        var posting = await OwnedPostings(userId).AsNoTracking().Include(item => item.Requirements)
            .Include(item => item.Analyses.OrderByDescending(analysis => analysis.Version).Take(1))
            .AsSingleQuery().SingleOrDefaultAsync(item => item.Id == postingId, cancellationToken);
        if (posting is null) return null;
        var latest = posting.Analyses.SingleOrDefault();
        return IsCurrent(latest, PostingAnalysisSource.From(posting).Fingerprint()) ? ToDto(latest!) : null;
    }

    public async Task<PostingAnalysisRequestResult> RequestAsync(string userId, int postingId, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var posting = await OwnedPostings(userId).Include(item => item.Requirements)
                .SingleOrDefaultAsync(item => item.Id == postingId, cancellationToken);
            if (posting is null) return new(null, PostingAnalysisRequestError.NotFound);
            if (posting.Status != PostingStatus.Published ||
                posting.ApplicationDeadline <= clock.GetUtcNow()) return new(null, PostingAnalysisRequestError.NotPublished);

            var analysis = await QueueAsync(posting, retryFailed: true, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(ToDto(analysis));
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        {
            return new(null, PostingAnalysisRequestError.Conflict);
        }
        catch (SqlException error) when (error.Number == 1205)
        {
            return new(null, PostingAnalysisRequestError.Conflict);
        }
        catch (InvalidOperationException error) when (error.GetBaseException() is SqlException { Number: 1205 })
        {
            return new(null, PostingAnalysisRequestError.Conflict);
        }
    }

    internal async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var analysis = await context.PostingAnalyses.Include(item => item.InternshipPosting).ThenInclude(posting => posting.Requirements)
            .Where(item => item.Status == AnalysisStatus.Pending).OrderBy(item => item.CreatedAt).ThenBy(item => item.Id)
            .AsSingleQuery().FirstOrDefaultAsync(cancellationToken);
        if (analysis is null) return false;
        analysis.Status = AnalysisStatus.Processing;
        var now = clock.GetUtcNow();
        analysis.StartedAt = now < analysis.CreatedAt ? analysis.CreatedAt : now;
        if (analysis.SchemaVersion != PostingAnalysisContract.SchemaVersion)
        {
            try { await FinishAsync(analysis, null, AnalysisErrorCode.Cancelled, cancellationToken); }
            catch (DbUpdateConcurrencyException) { }
            return true;
        }
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return true; }

        try
        {
            var posting = analysis.InternshipPosting;
            var source = PostingAnalysisSource.From(posting);
            if (posting.Status != PostingStatus.Published || posting.ApplicationDeadline <= clock.GetUtcNow() ||
                !IsCurrent(analysis, source.Fingerprint()) || await context.PostingAnalyses.AnyAsync(item =>
                    item.InternshipPostingId == posting.Id && item.Version > analysis.Version, cancellationToken))
            {
                await FinishAsync(analysis, null, AnalysisErrorCode.Cancelled, cancellationToken);
                return true;
            }

            string? correction = null;
            var request = new LlmJsonRequest(PostingAnalysisContract.Instruction, PostingAnalysisContract.CreateInput(source),
                PostingAnalysisContract.CreateSchema(), json => PostingAnalysisContract.Validate(json,
                    reason => logger.LogWarning("İlan yanıtı doğrulanamadı. AnalysisId: {AnalysisId}, Reason: {Reason}", analysis.Id, reason),
                    feedback => correction = feedback), "postingText", () => correction, initialOutputTokens: 1024);
            var response = await llm.GenerateJsonAsync(request, cancellationToken);
            await FinishAsync(analysis, response.Json, response.ErrorCode, cancellationToken);
        }
        catch (DbUpdateConcurrencyException) { }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { await FinishAsync(analysis, null, AnalysisErrorCode.Cancelled, shutdown.Token); }
            catch (Exception error) when (error is OperationCanceledException or DbUpdateException or SqlException) { }
            throw;
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            await FinishAsync(analysis, null, AnalysisErrorCode.ProcessingFailed, cancellationToken);
        }
        return true;
    }

    internal static bool IsCurrent(PostingAnalysis? analysis, string fingerprint) => analysis is not null &&
        analysis.SourceContentHash == fingerprint;

    private IQueryable<InternshipPosting> OwnedPostings(string userId) =>
        context.InternshipPostings.Where(item => item.CompanyProfile.IdentityUserId == userId);

    private Task<PostingAnalysis?> LatestAsync(int postingId, CancellationToken cancellationToken) => context.PostingAnalyses
        .Where(item => item.InternshipPostingId == postingId).OrderByDescending(item => item.Version).FirstOrDefaultAsync(cancellationToken);

    private async Task FinishAsync(PostingAnalysis analysis, string? json, AnalysisErrorCode? error, CancellationToken cancellationToken)
    {
        analysis.Status = json is not null && error is null ? AnalysisStatus.Completed : AnalysisStatus.Failed;
        analysis.DataJson = analysis.Status == AnalysisStatus.Completed ? json : null;
        analysis.ErrorCode = analysis.Status == AnalysisStatus.Completed ? null : error ?? AnalysisErrorCode.ProcessingFailed;
        var now = clock.GetUtcNow();
        analysis.CompletedAt = now < analysis.StartedAt ? analysis.StartedAt : now;
        await context.SaveChangesAsync(cancellationToken);
        if (analysis.ErrorCode.HasValue)
            logger.LogWarning("İlan analizi tamamlanamadı. AnalysisId: {AnalysisId}, ErrorCode: {ErrorCode}", analysis.Id, analysis.ErrorCode.Value);
    }

    private static PostingAnalysisDto ToDto(PostingAnalysis analysis) => new(analysis.Id, analysis.InternshipPostingId,
        analysis.Version, analysis.SchemaVersion, analysis.Status, analysis.CreatedAt, analysis.StartedAt, analysis.CompletedAt,
        analysis.ErrorCode, ErrorMessage(analysis.ErrorCode), analysis.SchemaVersion == PostingAnalysisContract.SchemaVersion ? PostingAnalysisContract.Read(analysis.DataJson) : null,
        analysis.SchemaVersion != PostingAnalysisContract.SchemaVersion);

    private static string? ErrorMessage(AnalysisErrorCode? error) => error switch
    {
        AnalysisErrorCode.InputTooLong => "İlan metni analiz sınırını aşıyor.",
        AnalysisErrorCode.InvalidResponse => "Model yanıtı beklenen ortak analiz şemasına uygun değil.",
        AnalysisErrorCode.Cancelled => "İlan analizi kesildi. Güncel ilan için yeniden analiz isteyebilirsiniz.",
        _ => AnalysisErrorMessages.Get(error)
    };
}

public enum PostingAnalysisRequestError { NotFound, NotPublished, Conflict }

public sealed record PostingAnalysisRequestResult(PostingAnalysisDto? Analysis, PostingAnalysisRequestError? Error = null);
