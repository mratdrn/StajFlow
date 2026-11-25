using System.Data;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class ResumeAnalysisService(ApplicationDbContext context, IPdfTextExtractionService extraction,
    ILlmService llm, ResumeTextRedactionService redaction, IOptions<ResumeStorageOptions> storage,
    IWebHostEnvironment environment, TimeProvider clock, ILogger<ResumeAnalysisService> logger) : IResumeAnalysisService
{
    public async Task<ResumeAnalysisDto?> GetAsync(string userId, int resumeId, CancellationToken cancellationToken)
    {
        var analysis = await context.ResumeAnalyses.AsNoTracking()
            .Where(record => record.ResumeId == resumeId && record.Resume.StudentProfile.IdentityUserId == userId)
            .OrderByDescending(record => record.Version).FirstOrDefaultAsync(cancellationToken);
        return analysis is null ? null : ToDto(analysis);
    }

    public async Task<ResumeAnalysisRequestResult> RequestAsync(string userId, int resumeId, CancellationToken cancellationToken)
    {
        await using var transaction = context.Database.CurrentTransaction is null
            ? await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken) : null;
        if (!await context.Resumes.AnyAsync(record => record.Id == resumeId && record.StudentProfile.IdentityUserId == userId, cancellationToken))
            return new(null, true);
        var existing = await context.ResumeAnalyses.Where(record => record.ResumeId == resumeId)
            .OrderByDescending(record => record.Version).ToListAsync(cancellationToken);
        var reusable = existing.FirstOrDefault(record => record.Status == AnalysisStatus.Completed && record.SchemaVersion == "1") ??
            existing.FirstOrDefault(record => record.Status is AnalysisStatus.Pending or AnalysisStatus.Processing);
        if (reusable is not null) return new(ToDto(reusable));
        var analysis = new ResumeAnalysis { ResumeId = resumeId, Version = existing.Count == 0 ? 1 : existing.Max(record => record.Version) + 1 };
        context.ResumeAnalyses.Add(analysis);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            return new(ToDto(analysis));
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        {
            return new(null);
        }
        catch (SqlException error) when (error.Number == 1205)
        {
            return new(null);
        }
        catch (InvalidOperationException error) when (error.GetBaseException() is SqlException { Number: 1205 })
        {
            return new(null);
        }
    }

    public async Task<bool> ProcessNextAsync(CancellationToken cancellationToken)
    {
        var analysis = await context.ResumeAnalyses.Include(record => record.Resume).ThenInclude(record => record.StudentProfile)
            .Where(record => record.Status == AnalysisStatus.Pending).OrderBy(record => record.CreatedAt).ThenBy(record => record.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (analysis is null) return false;
        analysis.Status = AnalysisStatus.Processing;
        var now = clock.GetUtcNow();
        analysis.StartedAt = now < analysis.CreatedAt ? analysis.CreatedAt : now;
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return true; }
        try
        {
            var resume = analysis.Resume;
            PdfTextExtractionResult extracted;
            var path = storage.Value.GetStoragePath(environment.ContentRootPath, resume.StorageKey);
            await using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
            {
                if (stream.Length > storage.Value.MaxFileSizeBytes)
                {
                    await FinishAsync(analysis, null, AnalysisErrorCode.FileTooLarge, cancellationToken);
                    return true;
                }
                analysis.SourceContentHash = Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken));
                stream.Position = 0;
                extracted = await extraction.ExtractAsync(stream, cancellationToken);
            }
            if (!extracted.Succeeded)
            {
                var error = extracted.Status switch
                {
                    PdfTextExtractionStatus.InvalidPdf => AnalysisErrorCode.InvalidPdf,
                    PdfTextExtractionStatus.EncryptedPdf => AnalysisErrorCode.EncryptedPdf,
                    PdfTextExtractionStatus.NoText => AnalysisErrorCode.NoText,
                    PdfTextExtractionStatus.UnreadableText => AnalysisErrorCode.UnreadableText,
                    PdfTextExtractionStatus.FileTooLarge => AnalysisErrorCode.FileTooLarge,
                    _ => AnalysisErrorCode.ReadFailed
                };
                await FinishAsync(analysis, null, error, cancellationToken);
                return true;
            }
            var student = resume.StudentProfile;
            var fullName = $"{student.FirstName} {student.LastName}";
            var text = redaction.Redact(extracted.Text!, fullName, student.Email, student.PhoneNumber);
            if (string.IsNullOrWhiteSpace(text) || !text.Any(char.IsLetter))
            {
                await FinishAsync(analysis, null, AnalysisErrorCode.UnreadableText, cancellationToken);
                return true;
            }
            var request = new LlmJsonRequest(ResumeAnalysisContract.Instruction, text, ResumeAnalysisContract.CreateSchema(),
                json => ResumeAnalysisContract.Validate(json, text, value => redaction.RemoveIdentifiers(value, fullName, student.Email, student.PhoneNumber),
                    reason => logger.LogWarning("CV yanıtı doğrulanamadı. AnalysisId: {AnalysisId}, Reason: {Reason}", analysis.Id, reason)));
            var result = await llm.GenerateJsonAsync(request, cancellationToken);
            await FinishAsync(analysis, result.Json, result.ErrorCode, cancellationToken);
        }
        catch (DbUpdateConcurrencyException) { }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            using var shutdown = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            try { await FinishAsync(analysis, null, AnalysisErrorCode.Cancelled, shutdown.Token); }
            catch (Exception error) when (error is OperationCanceledException or DbUpdateException or SqlException) { }
            throw;
        }
        catch (FileNotFoundException) { await FinishAsync(analysis, null, AnalysisErrorCode.FileNotFound, cancellationToken); }
        catch (DirectoryNotFoundException) { await FinishAsync(analysis, null, AnalysisErrorCode.FileNotFound, cancellationToken); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            await FinishAsync(analysis, null, AnalysisErrorCode.ReadFailed, cancellationToken);
        }
        catch (Exception error) when (error is not OutOfMemoryException)
        {
            await FinishAsync(analysis, null, AnalysisErrorCode.ProcessingFailed, cancellationToken);
        }
        return true;
    }

    private async Task FinishAsync(ResumeAnalysis analysis, string? json, AnalysisErrorCode? error, CancellationToken cancellationToken)
    {
        analysis.Status = json is not null && error is null ? AnalysisStatus.Completed : AnalysisStatus.Failed;
        analysis.DataJson = analysis.Status == AnalysisStatus.Completed ? json : null;
        analysis.ErrorCode = analysis.Status == AnalysisStatus.Completed ? null : error ?? AnalysisErrorCode.ProcessingFailed;
        var now = clock.GetUtcNow();
        analysis.CompletedAt = now < analysis.StartedAt ? analysis.StartedAt : now;
        await context.SaveChangesAsync(cancellationToken);
        if (analysis.ErrorCode.HasValue)
            logger.LogWarning("CV analizi tamamlanamadı. AnalysisId: {AnalysisId}, ErrorCode: {ErrorCode}", analysis.Id, analysis.ErrorCode.Value);
    }

    private static ResumeAnalysisDto ToDto(ResumeAnalysis analysis)
    {
        ResumeAnalysisData? data = null;
        var code = analysis.ErrorCode;
        if (analysis.Status == AnalysisStatus.Completed && analysis.DataJson is not null)
        {
            try { data = JsonSerializer.Deserialize<ResumeAnalysisData>(analysis.DataJson); }
            catch (JsonException) { code = AnalysisErrorCode.InvalidResponse; }
        }
        return new(analysis.Id, analysis.ResumeId, analysis.Version, analysis.Status.ToString(), analysis.CreatedAt,
            analysis.StartedAt, analysis.CompletedAt, code?.ToString(), AnalysisErrorMessages.Get(code), data);
    }
}
