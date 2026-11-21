using System.Linq.Expressions;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs.Applications;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class CompanyApplicationService(ApplicationDbContext context) : ICompanyApplicationService
{
    private static readonly Expression<Func<InternshipApplication, CompanyApplicationDto>> DtoProjection = application => new(
        application.Id, application.InternshipPostingId, application.InternshipPosting.Title,
        application.StudentProfile.FirstName + " " + application.StudentProfile.LastName, application.StudentProfile.Email,
        application.StudentProfile.University, application.StudentProfile.Department, application.Resume.OriginalFileName,
        application.CoverLetter, application.Status.ToString(), application.AppliedAt, application.StatusUpdatedAt,
        Convert.ToBase64String(application.RowVersion));

    public async Task<IReadOnlyList<CompanyApplicationDto>> ListAsync(string userId, int? postingId, CancellationToken cancellationToken) =>
        await BuildListQuery(userId, postingId).ToListAsync(cancellationToken);

    public async Task<CompanyApplicationPageDto> ListPageAsync(
        string userId, int? postingId, int page, string? status, CancellationToken cancellationToken)
    {
        var counts = await BuildApplicationQuery(userId, postingId).GroupBy(application => application.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() }).ToListAsync(cancellationToken);
        var statusCounts = counts.ToDictionary(group => group.Status.ToString(), group => group.Count);
        statusCounts["All"] = counts.Sum(group => group.Count);
        var result = await BuildListQuery(userId, postingId, status).ToPageAsync(page, 50, cancellationToken);
        return new(result.Items, result.Page, result.PageSize, result.TotalCount, statusCounts);
    }

    private IQueryable<InternshipApplication> BuildApplicationQuery(string userId, int? postingId)
    {
        var query = context.Applications.AsNoTracking()
            .Where(application => application.InternshipPosting.CompanyProfile.IdentityUserId == userId);
        if (postingId.HasValue) query = query.Where(application => application.InternshipPostingId == postingId.Value);
        return query;
    }

    private IQueryable<CompanyApplicationDto> BuildListQuery(string userId, int? postingId, string? status = null)
    {
        var query = BuildApplicationQuery(userId, postingId);
        if (status is not null)
        {
            var selectedStatus = Enum.Parse<ApplicationStatus>(status);
            query = query.Where(application => application.Status == selectedStatus);
        }
        return query.OrderByDescending(application => application.AppliedAt).ThenByDescending(application => application.Id)
            .Select(DtoProjection);
    }

    public Task<CompanyApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken) =>
        context.Applications.AsNoTracking().Where(application => application.Id == id &&
                application.InternshipPosting.CompanyProfile.IdentityUserId == userId)
            .Select(DtoProjection).SingleOrDefaultAsync(cancellationToken);

    public async Task<ApplicationStatusResult> UpdateStatusAsync(string userId, int id, UpdateApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        var application = await context.Applications.SingleOrDefaultAsync(application => application.Id == id &&
            application.InternshipPosting.CompanyProfile.IdentityUserId == userId, cancellationToken);
        if (application is null) return new(null, ApplicationStatusError.NotFound);

        var version = Convert.FromBase64String(request.RowVersion);
        if (!application.RowVersion.SequenceEqual(version)) return new(null, ApplicationStatusError.VersionConflict);
        if (application.Status == ApplicationStatus.Withdrawn) return new(null, ApplicationStatusError.Withdrawn);

        var newStatus = Enum.Parse<ApplicationStatus>(request.Status);
        if (application.Status == newStatus) return new(await GetAsync(userId, id, cancellationToken));
        application.Status = newStatus;
        application.StatusUpdatedAt = DateTimeOffset.UtcNow;
        context.Entry(application).Property(application => application.RowVersion).OriginalValue = version;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return new(await GetAsync(userId, id, cancellationToken));
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(null, ApplicationStatusError.VersionConflict);
        }
    }
}
