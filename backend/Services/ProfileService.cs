using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Profiles;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class ProfileService(ApplicationDbContext context) : IProfileService
{
    public Task<StudentProfileDto?> GetStudentAsync(string userId, CancellationToken cancellationToken) =>
        context.StudentProfiles.AsNoTracking().Where(profile => profile.IdentityUserId == userId)
            .Select(profile => new StudentProfileDto(profile.Id, profile.FirstName, profile.LastName,
                profile.Email, profile.University, profile.Department, profile.PhoneNumber, profile.Biography))
            .SingleOrDefaultAsync(cancellationToken);

    public Task<CompanyProfileDto?> GetCompanyAsync(string userId, CancellationToken cancellationToken) =>
        context.CompanyProfiles.AsNoTracking().Where(profile => profile.IdentityUserId == userId)
            .Select(profile => new CompanyProfileDto(profile.Id, profile.Name, profile.ContactEmail,
                profile.Industry, profile.Location, profile.Description, profile.WebsiteUrl))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<StudentProfileDto?> UpdateStudentAsync(string userId,
        UpdateStudentProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await context.StudentProfiles.SingleOrDefaultAsync(
            profile => profile.IdentityUserId == userId, cancellationToken);
        if (profile is null) return null;

        profile.FirstName = request.FirstName.Trim();
        profile.LastName = request.LastName.Trim();
        profile.PhoneNumber = TrimToNull(request.PhoneNumber);
        profile.University = TrimToNull(request.University);
        profile.Department = TrimToNull(request.Department);
        profile.Biography = TrimToNull(request.Biography);
        await context.SaveChangesAsync(cancellationToken);
        return await GetStudentAsync(userId, cancellationToken);
    }

    public async Task<CompanyProfileDto?> UpdateCompanyAsync(string userId,
        UpdateCompanyProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await context.CompanyProfiles.SingleOrDefaultAsync(
            profile => profile.IdentityUserId == userId, cancellationToken);
        if (profile is null) return null;

        profile.Name = request.Name.Trim();
        profile.Description = TrimToNull(request.Description);
        profile.WebsiteUrl = TrimToNull(request.WebsiteUrl);
        profile.Industry = TrimToNull(request.Industry);
        profile.Location = TrimToNull(request.Location);
        await context.SaveChangesAsync(cancellationToken);
        return await GetCompanyAsync(userId, cancellationToken);
    }

    private static string? TrimToNull(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}

