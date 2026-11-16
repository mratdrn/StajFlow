using StajFlow.Api.DTOs.Applications;

namespace StajFlow.Api.Services.Interfaces;

public interface IStudentApplicationService
{
    Task<IReadOnlyList<InternshipApplicationDto>> ListAsync(string userId, CancellationToken cancellationToken);
    Task<InternshipApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken);
    Task<ApplicationCreationResult> CreateAsync(string userId, CreateApplicationRequest request, CancellationToken cancellationToken);
}

public enum ApplicationCreationError { NotFound, InvalidResume, AlreadyApplied, DeadlinePassed }
public record ApplicationCreationResult(InternshipApplicationDto? Application, ApplicationCreationError? Error = null);
