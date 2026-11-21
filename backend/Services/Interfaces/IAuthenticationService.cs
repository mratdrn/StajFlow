using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using StajFlow.Api.DTOs.Auth;

namespace StajFlow.Api.Services.Interfaces;

public interface IAuthenticationService
{
    Task<RegistrationResult> RegisterStudentAsync(StudentRegistrationRequest request, CancellationToken cancellationToken);
    Task<RegistrationResult> RegisterCompanyAsync(CompanyRegistrationRequest request, CancellationToken cancellationToken);
    Task<bool> LoginAsync(LoginRequest request);
    Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal principal, ChangePasswordRequest request);
    Task<bool> LogoutAsync(ClaimsPrincipal principal);
    Task<CompanyAccountDeletionResult> DeleteCompanyAccountAsync(ClaimsPrincipal principal, string password, CancellationToken cancellationToken);
    Task<CurrentUserDto?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

