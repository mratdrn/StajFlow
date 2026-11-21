using StajFlow.Api.DTOs;

namespace StajFlow.Api.Services.Interfaces;

public interface IResumeService
{
    Task<IReadOnlyList<ResumeDto>> ListAsync(string userId, CancellationToken cancellationToken);
    Task<ResumeUploadResult> UploadAsync(string userId, IFormFile file, CancellationToken cancellationToken);
    Task<ResumeDeletionResult> DeleteAsync(string userId, int id, CancellationToken cancellationToken);
    Task<ResumeFile?> GetFileAsync(string userId, int id, CancellationToken cancellationToken);
    Task<ResumeFile?> GetCompanyApplicationFileAsync(string userId, int applicationId, CancellationToken cancellationToken);
}

public record ResumeUploadResult(ResumeDto? Resume, string? Error = null, bool ProfileNotFound = false);
public record ResumeFile(Stream Content, string FileName);

public enum ResumeDeletionResult { Succeeded, NotFound, InUse, Failed }
