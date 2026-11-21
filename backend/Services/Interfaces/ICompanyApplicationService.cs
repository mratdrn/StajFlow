using StajFlow.Api.DTOs.Applications;

namespace StajFlow.Api.Services.Interfaces;

public interface ICompanyApplicationService
{
    Task<IReadOnlyList<CompanyApplicationDto>> ListAsync(string userId, int? postingId, CancellationToken cancellationToken);
    Task<CompanyApplicationPageDto> ListPageAsync(string userId, int? postingId, int page, string? status, CancellationToken cancellationToken);
    Task<CompanyApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken);
    Task<ApplicationStatusResult> UpdateStatusAsync(string userId, int id, UpdateApplicationStatusRequest request, CancellationToken cancellationToken);
}

public enum ApplicationStatusError { NotFound, VersionConflict, Withdrawn }
public record ApplicationStatusResult(CompanyApplicationDto? Application, ApplicationStatusError? Error = null);
