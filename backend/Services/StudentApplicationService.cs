using System.Data;
using System.Linq.Expressions;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Applications;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class StudentApplicationService(ApplicationDbContext context, CompatibilityService compatibility, TimeProvider clock)
    : IStudentApplicationService
{
    private static readonly Expression<Func<InternshipApplication, InternshipApplicationDto>> DtoProjection = application => new(
        application.Id, application.InternshipPostingId, application.InternshipPosting.Title, application.InternshipPosting.CompanyProfile.Name,
        application.InternshipPosting.Location, application.InternshipPosting.WorkArrangement.ToString(), application.InternshipPosting.Status.ToString(),
        application.ResumeId, application.Resume.OriginalFileName, application.CoverLetter, application.Status.ToString(),
        application.AppliedAt, application.StatusUpdatedAt, application.InternshipPosting.ApplicationDeadline,
        application.Evaluation == null ? null : new ApplicationEvaluationDto(application.Evaluation.SchemaVersion,
            application.Evaluation.ScoringRuleVersion, application.Evaluation.Status, application.Evaluation.Score,
            application.Evaluation.UnavailableReason, application.Evaluation.RequiredCriteriaStatus ?? RequiredCriteriaStatus.Unavailable,
            application.Evaluation.EvaluatedAt, null));

    public async Task<IReadOnlyList<InternshipApplicationDto>> ListAsync(string userId, CancellationToken cancellationToken) =>
        await OrderForList(BuildApplicationQuery(userId).Where(application => application.Status != ApplicationStatus.Withdrawn))
            .ToListAsync(cancellationToken);

    public async Task<StudentApplicationPageDto> ListPageAsync(
        string userId, int page, string? status, CancellationToken cancellationToken)
    {
        var query = BuildApplicationQuery(userId);
        var counts = await query.GroupBy(application => application.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() }).ToListAsync(cancellationToken);
        var statusCounts = counts.ToDictionary(group => group.Status.ToString(), group => group.Count);
        statusCounts["All"] = counts.Sum(group => group.Count);
        if (status is not null)
        {
            var selectedStatus = Enum.Parse<ApplicationStatus>(status);
            query = query.Where(application => application.Status == selectedStatus);
        }
        var result = await OrderForList(query).ToPageAsync(page, 6, cancellationToken);
        return new(result.Items, result.Page, result.PageSize, result.TotalCount, statusCounts);
    }

    private IQueryable<InternshipApplication> BuildApplicationQuery(string userId) =>
        context.Applications.AsNoTracking()
            .Where(application => application.StudentProfile.IdentityUserId == userId);

    private static IQueryable<InternshipApplicationDto> OrderForList(IQueryable<InternshipApplication> query) =>
        query.OrderByDescending(application => application.AppliedAt).ThenByDescending(application => application.Id)
            .Select(DtoProjection);

    public async Task<InternshipApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var query = BuildApplicationQuery(userId).Where(application => application.Id == id);
        var result = await query.Select(DtoProjection).SingleOrDefaultAsync(cancellationToken);
        if (result?.Evaluation is not { } evaluation) return result;
        var json = await query.Select(application => application.Evaluation!.ResultJson).SingleOrDefaultAsync(cancellationToken);
        return result with { Evaluation = evaluation.WithData(json) };
    }

    public Task<InternshipPostingDetailDto?> GetPostingAsync(string userId, int id, CancellationToken cancellationToken) =>
        BuildApplicationQuery(userId).Where(application => application.Id == id)
            .Select(application => application.InternshipPosting)
            .Select(InternshipPostingService.DetailProjection).SingleOrDefaultAsync(cancellationToken);

    public async Task<ApplicationWithdrawalResult> WithdrawAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var application = await context.Applications.SingleOrDefaultAsync(application =>
            application.Id == id && application.StudentProfile.IdentityUserId == userId, cancellationToken);
        if (application is null) return new(ApplicationWithdrawalError.NotFound);
        if (application.Status == ApplicationStatus.Withdrawn) return new();
        if (application.Status is not (ApplicationStatus.Submitted or ApplicationStatus.UnderReview))
            return new(ApplicationWithdrawalError.InvalidStatus);
        application.Status = ApplicationStatus.Withdrawn;
        var now = clock.GetUtcNow();
        application.StatusUpdatedAt = now < application.AppliedAt ? application.AppliedAt : now;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return new();
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(ApplicationWithdrawalError.VersionConflict);
        }
    }

    public async Task<ApplicationCreationResult> CreateAsync(string userId, CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            var studentId = await context.StudentProfiles.AsNoTracking().Where(student => student.IdentityUserId == userId)
                .Select(student => (int?)student.Id).SingleOrDefaultAsync(cancellationToken);
            if (studentId is null) return new(null, ApplicationCreationError.NotFound);

            if (await context.Applications.AsNoTracking().AnyAsync(application =>
                    application.StudentProfileId == studentId && application.InternshipPostingId == request.InternshipPostingId &&
                    application.Status != ApplicationStatus.Withdrawn, cancellationToken))
                return new(null, ApplicationCreationError.AlreadyApplied);

            var posting = await CompatibilityService.WithCriterionSource(context.InternshipPostings.AsNoTracking())
                .SingleOrDefaultAsync(posting => posting.Id == request.InternshipPostingId && posting.Status == PostingStatus.Published,
                    cancellationToken);
            if (posting is null) return new(null, ApplicationCreationError.NotFound);
            var resume = await CompatibilityService.WithResumeSource(context.Resumes.AsNoTracking())
                .SingleOrDefaultAsync(resume => resume.Id == request.ResumeId && resume.StudentProfileId == studentId &&
                    resume.ContentType == "application/pdf", cancellationToken);
            if (resume is null) return new(null, ApplicationCreationError.InvalidResume);

            var now = clock.GetUtcNow();
            if (posting.ApplicationDeadline <= now) return new(null, ApplicationCreationError.DeadlinePassed);

            var application = new InternshipApplication
            {
                StudentProfileId = studentId.Value,
                InternshipPostingId = posting.Id,
                ResumeId = resume.Id,
                CoverLetter = string.IsNullOrWhiteSpace(request.CoverLetter) ? null : request.CoverLetter.Trim(),
                Status = ApplicationStatus.Submitted,
                AppliedAt = now,
                Evaluation = compatibility.CreateSnapshot(resume, posting, now)
            };
            context.Applications.Add(application);
            await context.SaveChangesAsync(cancellationToken);
            var result = await GetAsync(userId, application.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(result);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return new(null, ApplicationCreationError.AlreadyApplied);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 1205 })
        {
            return new(null, ApplicationCreationError.VersionConflict);
        }
        catch (SqlException error) when (error.Number == 1205)
        {
            return new(null, ApplicationCreationError.VersionConflict);
        }
    }
}
