using System.Security.Claims;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs.Auth;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class AuthenticationService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager) : IAuthenticationService
{
    public Task<RegistrationResult> RegisterStudentAsync(StudentRegistrationRequest request, CancellationToken cancellationToken)
    {
        var parts = request.FullName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return Task.FromResult(RegistrationResult.Invalid("FullName", "Adınızı ve soyadınızı girin."));
        var name = string.Join(" ", parts[..^1]);
        var lastName = parts[^1];
        if (name.Length > 100 || lastName.Length > 100)
            return Task.FromResult(RegistrationResult.Invalid("FullName", "Ad ve soyad ayrı ayrı en fazla 100 karakter olabilir."));

        return RegisterAsync(request.Email, request.Password, UserRoles.Student, user =>
            context.StudentProfiles.Add(new StudentProfile
            {
                IdentityUserId = user.Id, FirstName = name, LastName = lastName, Email = user.Email!
            }), cancellationToken);
    }

    public Task<RegistrationResult> RegisterCompanyAsync(CompanyRegistrationRequest request, CancellationToken cancellationToken)
    {
        var companyName = request.CompanyName.Trim();
        if (companyName.Length == 0)
            return Task.FromResult(RegistrationResult.Invalid("CompanyName", "Şirket adını girin."));

        return RegisterAsync(request.Email, request.Password, UserRoles.Company, user =>
            context.CompanyProfiles.Add(new CompanyProfile
            {
                IdentityUserId = user.Id, Name = companyName, ContactEmail = user.Email!
            }), cancellationToken);
    }

    private async Task<RegistrationResult> RegisterAsync(string email, string password, string role,
        Action<ApplicationUser> addProfile, CancellationToken cancellationToken)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var user = new ApplicationUser { Email = normalizedEmail, UserName = normalizedEmail };
            var created = await userManager.CreateAsync(user, password);
            if (!created.Succeeded)
            {
                var errors = created.Errors
                    .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal) ? "Password" : "Email")
                    .ToDictionary(group => group.Key, group => group.Select(error =>
                        error.Code.StartsWith("Duplicate", StringComparison.Ordinal)
                            ? "Bu e-posta adresiyle zaten bir hesap bulunuyor."
                            : error.Description).Distinct().ToArray());
                return new RegistrationResult(false, errors);
            }

            var assigned = await userManager.AddToRoleAsync(user, role);
            if (!assigned.Succeeded)
                throw new InvalidOperationException("Kayıt sırasında kullanıcı rolü atanamadı.");

            addProfile(user);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return RegistrationResult.Success();
        }
        catch (DbUpdateException exception) when (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();
            return RegistrationResult.Invalid("Email", "Bu e-posta adresiyle zaten bir hesap bulunuyor.");
        }
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null) return false;
        var result = await signInManager.PasswordSignInAsync(user, request.Password,
            isPersistent: false, lockoutOnFailure: true);
        return result.Succeeded;
    }

    public async Task<bool> LogoutAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null) return false;
        var result = await userManager.UpdateSecurityStampAsync(user);
        if (!result.Succeeded) return false;
        await signInManager.SignOutAsync();
        return true;
    }

    public async Task<CurrentUserDto?> GetCurrentUserAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user?.Email is null) return null;
        if (await userManager.IsInRoleAsync(user, UserRoles.Student))
        {
            return await context.StudentProfiles.AsNoTracking().Where(profile => profile.IdentityUserId == user.Id)
                .Select(profile => new CurrentUserDto(user.Id, user.Email,
                    profile.FirstName + " " + profile.LastName, UserRoles.Student, profile.Id))
                .SingleOrDefaultAsync(cancellationToken);
        }

        if (await userManager.IsInRoleAsync(user, UserRoles.Company))
        {
            return await context.CompanyProfiles.AsNoTracking().Where(profile => profile.IdentityUserId == user.Id)
                .Select(profile => new CurrentUserDto(user.Id, user.Email, profile.Name, UserRoles.Company, profile.Id))
                .SingleOrDefaultAsync(cancellationToken);
        }
        return null;
    }
}

