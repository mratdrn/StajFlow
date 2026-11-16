using System.Security.Claims;
using StajFlow.Api.DTOs.Auth;

namespace StajFlow.Api.Services.Interfaces;

public interface IAuthenticationService
{
    Task<RegistrationResult> RegisterStudentAsync(StudentRegistrationRequest request, CancellationToken cancellationToken);
    Task<RegistrationResult> RegisterCompanyAsync(CompanyRegistrationRequest request, CancellationToken cancellationToken);
    Task<bool> LoginAsync(LoginRequest request);
    Task<bool> LogoutAsync(ClaimsPrincipal principal);
    Task<CurrentUserDto?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}

