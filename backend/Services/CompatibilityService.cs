using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Services;

public sealed class CompatibilityService(ApplicationDbContext context, CompatibilityScorer scorer, TimeProvider clock)
{
    public async Task<CompatibilityDto?> GetAsync(string userId, int postingId, int? resumeId, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var posting = await WithCriterionSource(context.InternshipPostings.AsNoTracking())
            .SingleOrDefaultAsync(item => item.Id == postingId && item.Status == PostingStatus.Published &&
                (item.ApplicationDeadline == null || item.ApplicationDeadline > now), cancellationToken);
        if (posting is null) return null;

        var resumes = context.Resumes.AsNoTracking().Where(item => item.StudentProfile.IdentityUserId == userId);
        if (resumeId.HasValue) resumes = resumes.Where(item => item.Id == resumeId.Value);
        var resume = await WithResumeSource(resumes).OrderByDescending(item => item.IsFavorite)
            .ThenBy(item => item.UploadedAt).ThenBy(item => item.Id).FirstOrDefaultAsync(cancellationToken);
        if (resumeId.HasValue && resume is null) return null;

        var evaluation = EvaluateSources(resume, posting);

        return new(postingId, resume?.Id, CompatibilityScorer.RuleVersion,
            evaluation.Score.HasValue ? EvaluationStatus.Calculated : EvaluationStatus.NotCalculated,
            evaluation.Score, evaluation.UnavailableReason, UnavailableMessage(evaluation.UnavailableReason),
            evaluation.Data.ResumeAnalysis, evaluation.Data.PostingAnalysis,
            evaluation.Data.Criteria.Where(item => item.Type == RequirementType.Required).ToList(),
            evaluation.Data.Criteria.Where(item => item.Type == RequirementType.Preferred).ToList(),
            evaluation.Data.MandatoryWarnings, evaluation.Data.SocialIndicators);
    }

    internal static IQueryable<InternshipPosting> WithCriterionSource(IQueryable<InternshipPosting> query) => query
        .Include(posting => posting.Requirements)
        .Include(posting => posting.Analyses.OrderByDescending(analysis => analysis.Version).Take(1)).AsSingleQuery();

    internal static IQueryable<Resume> WithResumeSource(IQueryable<Resume> query) => query
        .Include(resume => resume.Analyses.OrderByDescending(analysis => analysis.Version).Take(1)).AsSingleQuery();

    internal ApplicationEvaluation CreateSnapshot(Resume resume, InternshipPosting posting, DateTimeOffset evaluatedAt)
    {
        var evaluation = EvaluateSources(resume, posting);
        var required = evaluation.Data.Criteria.Where(item => item.Type == RequirementType.Required).ToList();
        var requiredStatus = evaluation.UnavailableReason switch
        {
            EvaluationUnavailableReason.NoMeasurableCriteria => RequiredCriteriaStatus.NoRequiredCriteria,
            not null => RequiredCriteriaStatus.Unavailable,
            _ when required.Count == 0 => RequiredCriteriaStatus.NoRequiredCriteria,
            _ when required.Any(item => item.MatchStatus == CriterionMatchStatus.NoEvidence) => RequiredCriteriaStatus.MissingEvidence,
            _ => RequiredCriteriaStatus.AllMatched
        };
        return new()
        {
            ResumeId = resume.Id,
            InternshipPostingId = posting.Id,
            ResumeAnalysisId = evaluation.Data.ResumeAnalysis?.Id,
            PostingAnalysisId = evaluation.Data.PostingAnalysis?.Id,
            ScoringRuleVersion = CompatibilityScorer.RuleVersion,
            Status = evaluation.Score.HasValue ? EvaluationStatus.Calculated : EvaluationStatus.NotCalculated,
            Score = evaluation.Score,
            UnavailableReason = evaluation.UnavailableReason,
            RequiredCriteriaStatus = requiredStatus,
            ResultJson = JsonSerializer.Serialize(evaluation.Data, JsonSerializerOptions.Web),
            EvaluatedAt = evaluatedAt
        };
    }

    private CompatibilityScoringResult EvaluateSources(Resume? resume, InternshipPosting posting)
    {
        var postingAnalysis = posting.Analyses.SingleOrDefault();
        if (!PostingAnalysisService.IsCurrent(postingAnalysis, PostingAnalysisSource.From(posting).Fingerprint()))
            postingAnalysis = null;
        var resumeAnalysis = resume?.Analyses.SingleOrDefault();
        var evaluation = Evaluate(resumeAnalysis, postingAnalysis);
        return evaluation with
        {
            Data = evaluation.Data with
            {
                ResumeAnalysis = resumeAnalysis is null ? null : new(resumeAnalysis.Id, resumeAnalysis.Version,
                    resumeAnalysis.SchemaVersion, resumeAnalysis.Status, resumeAnalysis.ErrorCode),
                PostingAnalysis = postingAnalysis is null ? null : new(postingAnalysis.Id, postingAnalysis.Version,
                    postingAnalysis.SchemaVersion, postingAnalysis.Status, postingAnalysis.ErrorCode,
                    Convert.ToBase64String(postingAnalysis.SourceRowVersion))
            }
        };
    }

    private CompatibilityScoringResult Evaluate(ResumeAnalysis? resume, PostingAnalysis? posting)
    {
        var status = UnavailableReason(resume?.Status, posting?.Status);
        if (!status.HasValue && (resume?.SchemaVersion == ResumeAnalysisContract.SchemaVersion && posting?.SchemaVersion == PostingAnalysisContract.SchemaVersion))
        {
            var resumeData = ResumeAnalysisContract.Read(resume.DataJson);
            var postingData = PostingAnalysisContract.Read(posting!.DataJson);
            if (resumeData is not null && postingData is not null) return scorer.Evaluate(resumeData, postingData, DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime));
        }
        return new(null, status ?? EvaluationUnavailableReason.InsufficientData, new()
        {
            ResumeAnalysisStatus = resume?.Status,
            PostingAnalysisStatus = posting?.Status
        });
    }

    private static EvaluationUnavailableReason? UnavailableReason(AnalysisStatus? resume, AnalysisStatus? posting)
    {
        if (resume == AnalysisStatus.Failed || posting == AnalysisStatus.Failed) return EvaluationUnavailableReason.AnalysisFailed;
        if (resume is AnalysisStatus.Pending or AnalysisStatus.Processing || posting is AnalysisStatus.Pending or AnalysisStatus.Processing)
            return EvaluationUnavailableReason.AnalysisPending;
        return resume == AnalysisStatus.Completed && posting == AnalysisStatus.Completed ? null : EvaluationUnavailableReason.InsufficientData;
    }

    private static string? UnavailableMessage(EvaluationUnavailableReason? reason) => reason switch
    {
        EvaluationUnavailableReason.AnalysisPending => "Uyumluluk hesaplanamadı. Analiz henüz tamamlanmadı.",
        EvaluationUnavailableReason.AnalysisFailed => "Uyumluluk hesaplanamadı. Gerekli analiz tamamlanamadı.",
        EvaluationUnavailableReason.NoMeasurableCriteria => "Uyumluluk hesaplanamadı. İlanda ölçülebilir kriter bulunamadı.",
        EvaluationUnavailableReason.InsufficientData => "Uyumluluk hesaplanamadı. Güncel şemaya uygun CV ve ilan analizi gerekiyor. Eski analizleri yeniden başlatın.",
        _ => null
    };
}
