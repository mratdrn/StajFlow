using StajFlow.Api.DTOs.Analysis;

namespace StajFlow.Api.Services.Interfaces;

public interface IResumeAnalysisService
{
    Task<ResumeAnalysisDto?> GetAsync(string userId, int resumeId, CancellationToken cancellationToken);
    Task<ResumeAnalysisRequestResult> RequestAsync(string userId, int resumeId, CancellationToken cancellationToken);
}

public record ResumeAnalysisRequestResult(ResumeAnalysisDto? Analysis, bool NotFound = false);
