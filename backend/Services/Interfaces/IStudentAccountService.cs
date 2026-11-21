using System.Security.Claims;

namespace StajFlow.Api.Services.Interfaces;

public interface IStudentAccountService
{
    Task<StudentAccountDeletionResult> DeleteAsync(ClaimsPrincipal principal, string password, CancellationToken cancellationToken);
}

public enum StudentAccountDeletionResult { Succeeded, NotFound, InvalidPassword, Conflict, Failed }
