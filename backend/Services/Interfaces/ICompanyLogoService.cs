namespace StajFlow.Api.Services.Interfaces;

public interface ICompanyLogoService
{
    Task<CompanyLogoUploadResult> UploadAsync(string userId, IFormFile file, CancellationToken cancellationToken);
    Task<CompanyLogoFile?> GetAsync(int companyId, CancellationToken cancellationToken);
}

public record CompanyLogoUploadResult(string? LogoUrl, string? Error = null, bool ProfileNotFound = false);
public record CompanyLogoFile(byte[] Data, string ContentType);
