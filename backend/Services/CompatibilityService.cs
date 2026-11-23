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
        var posting = await context.InternshipPostings.AsNoTracking().Where(item => item.Id == postingId &&
                item.Status == PostingStatus.Published && (item.ApplicationDeadline == null || item.ApplicationDeadline > now))
            .Include(item => item.Requirements).Include(item => item.Analyses.OrderByDescending(analysis => analysis.Version).Take(1))
            .AsSingleQuery().SingleOrDefaultAsync(cancellationToken);
        if (posting is null) return null;

        var resumes = context.Resumes.AsNoTracking().Where(item => item.StudentProfile.IdentityUserId == userId);
        if (resumeId.HasValue) resumes = resumes.Where(item => item.Id == resumeId.Value);
        var resume = await resumes.OrderByDescending(item => item.UploadedAt).ThenByDescending(item => item.Id)
            .Include(item => item.Analyses.OrderByDescending(analysis => analysis.Version).Take(1))
            .AsSingleQuery().FirstOrDefaultAsync(cancellationToken);
        if (resumeId.HasValue && resume is null) return null;

        var latestPosting = posting.Analyses.SingleOrDefault();
        if (!PostingAnalysisService.IsCurrent(latestPosting, PostingAnalysisSource.From(posting).Fingerprint())) latestPosting = null;
        var latestResume = resume?.Analyses.SingleOrDefault();
        var evaluation = Evaluate(latestResume, latestPosting);

        return new(postingId, resume?.Id, CompatibilityScorer.RuleVersion,
            evaluation.Score.HasValue ? EvaluationStatus.Calculated : EvaluationStatus.NotCalculated,
            evaluation.Score, evaluation.UnavailableReason, UnavailableMessage(evaluation.UnavailableReason),
            latestResume is null ? null : new(latestResume.Id, latestResume.Version, latestResume.SchemaVersion, latestResume.Status, latestResume.ErrorCode),
            latestPosting is null ? null : new(latestPosting.Id, latestPosting.Version, latestPosting.SchemaVersion, latestPosting.Status,
                latestPosting.ErrorCode, Convert.ToBase64String(latestPosting.SourceRowVersion)),
            evaluation.Data.Criteria.Where(item => item.Type == RequirementType.Required).ToList(),
            evaluation.Data.Criteria.Where(item => item.Type == RequirementType.Preferred).ToList(),
            evaluation.Data.MandatoryWarnings, evaluation.Data.SocialIndicators);
    }

    private CompatibilityScoringResult Evaluate(ResumeAnalysis? resume, PostingAnalysis? posting)
    {
        var status = UnavailableReason(resume?.Status, posting?.Status);
        if (!status.HasValue && resume?.SchemaVersion == "1")
        {
            var resumeData = AnalysisJson.Read<ResumeAnalysisData>(resume.DataJson);
            var postingData = PostingAnalysisContract.Read(posting!.DataJson);
            if (resumeData is not null && postingData is not null) return scorer.Evaluate(resumeData, postingData);
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
        EvaluationUnavailableReason.InsufficientData => "Uyumluluk hesaplanamadı. Güncel ilan veya CV analizi bulunamadı.",
        _ => null
    };
}
