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
        Convert.ToBase64String(application.RowVersion), application.ResumeId,
        application.Evaluation == null ? null : new ApplicationEvaluationDto(application.Evaluation.SchemaVersion,
            application.Evaluation.ScoringRuleVersion, application.Evaluation.Status, application.Evaluation.Score,
            application.Evaluation.UnavailableReason, application.Evaluation.RequiredCriteriaStatus ?? RequiredCriteriaStatus.Unavailable,
            application.Evaluation.EvaluatedAt, null));

    public async Task<IReadOnlyList<CompanyApplicationDto>> ListAsync(string userId, int? postingId, CancellationToken cancellationToken) =>
        await OrderForList(BuildApplicationQuery(userId, postingId), null).ToListAsync(cancellationToken);

    public async Task<CompanyApplicationPageDto> ListPageAsync(
        string userId, int? postingId, int page, string? status, CompanyApplicationFilters filters, CancellationToken cancellationToken)
    {
        var query = ApplyFilters(BuildApplicationQuery(userId, postingId), filters);
        var counts = await query.GroupBy(application => application.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() }).ToListAsync(cancellationToken);
        var statusCounts = counts.ToDictionary(group => group.Status.ToString(), group => group.Count);
        statusCounts["All"] = counts.Sum(group => group.Count);
        if (status is not null)
        {
            var selectedStatus = Enum.Parse<ApplicationStatus>(status);
            query = query.Where(application => application.Status == selectedStatus);
        }
        var result = await OrderForList(query, filters.Sort).ToPageAsync(page, 50, cancellationToken);
        return new(result.Items, result.Page, result.PageSize, result.TotalCount, statusCounts);
    }

    private IQueryable<InternshipApplication> BuildApplicationQuery(string userId, int? postingId)
    {
        var query = context.Applications.AsNoTracking()
            .Where(application => application.InternshipPosting.CompanyProfile.IdentityUserId == userId);
        if (postingId.HasValue) query = query.Where(application => application.InternshipPostingId == postingId.Value);
        return query;
    }

    private static IQueryable<InternshipApplication> ApplyFilters(IQueryable<InternshipApplication> query, CompanyApplicationFilters filters)
    {
        if (filters.ScoreStatus == "Calculated")
            query = query.Where(application => application.Evaluation != null && application.Evaluation.Score.HasValue);
        if (filters.ScoreStatus == "NotCalculated")
            query = query.Where(application => application.Evaluation == null || !application.Evaluation.Score.HasValue);
        if (filters.MinScore is { } minimum)
            query = query.Where(application => application.Evaluation != null && application.Evaluation.Score >= minimum);
        if (filters.MaxScore is { } maximum)
            query = query.Where(application => application.Evaluation != null && application.Evaluation.Score <= maximum);
        if (filters.RequiredCriteriaStatus is { } required)
        {
            var state = Enum.Parse<RequiredCriteriaStatus>(required);
            query = state == RequiredCriteriaStatus.Unavailable
                ? query.Where(application => application.Evaluation == null || application.Evaluation.RequiredCriteriaStatus == null ||
                    application.Evaluation.RequiredCriteriaStatus == state)
                : query.Where(application => application.Evaluation != null && application.Evaluation.RequiredCriteriaStatus == state);
        }
        return query;
    }

    private static IQueryable<CompanyApplicationDto> OrderForList(IQueryable<InternshipApplication> query, string? sort)
    {
        if (sort is "ScoreAsc" or "ScoreDesc")
        {
            var ranked = query.OrderBy(application => application.Evaluation == null || !application.Evaluation.Score.HasValue);
            ranked = sort == "ScoreAsc"
                ? ranked.ThenBy(application => application.Evaluation!.Score)
                : ranked.ThenByDescending(application => application.Evaluation!.Score);
            return ranked.ThenByDescending(application => application.AppliedAt).ThenByDescending(application => application.Id)
                .Select(DtoProjection);
        }
        return query.OrderByDescending(application => application.AppliedAt).ThenByDescending(application => application.Id)
            .Select(DtoProjection);
    }

    public async Task<CompanyApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var query = BuildApplicationQuery(userId, null).Where(application => application.Id == id);
        var result = await query.Select(DtoProjection).SingleOrDefaultAsync(cancellationToken);
        if (result?.Evaluation is not { } evaluation) return result;
        var json = await query.Select(application => application.Evaluation!.ResultJson).SingleOrDefaultAsync(cancellationToken);
        return result with { Evaluation = evaluation.WithData(json) };
    }

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
        if (application.Status is ApplicationStatus.Accepted or ApplicationStatus.Rejected)
            return new(null, ApplicationStatusError.Finalized);
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
