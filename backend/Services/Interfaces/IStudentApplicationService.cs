using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Applications;

namespace StajFlow.Api.Services.Interfaces;

public interface IStudentApplicationService
{
    Task<IReadOnlyList<InternshipApplicationDto>> ListAsync(string userId, CancellationToken cancellationToken);
    Task<StudentApplicationPageDto> ListPageAsync(string userId, int page, string? status, CancellationToken cancellationToken);
    Task<InternshipApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken);
    Task<InternshipPostingDetailDto?> GetPostingAsync(string userId, int id, CancellationToken cancellationToken);
    Task<ApplicationWithdrawalResult> WithdrawAsync(string userId, int id, CancellationToken cancellationToken);
    Task<ApplicationCreationResult> CreateAsync(string userId, CreateApplicationRequest request, CancellationToken cancellationToken);
}

public enum ApplicationCreationError { NotFound, InvalidResume, AlreadyApplied, DeadlinePassed }
public record ApplicationCreationResult(InternshipApplicationDto? Application, ApplicationCreationError? Error = null);

public enum ApplicationWithdrawalError { NotFound, InvalidStatus, VersionConflict }
public record ApplicationWithdrawalResult(ApplicationWithdrawalError? Error = null);
