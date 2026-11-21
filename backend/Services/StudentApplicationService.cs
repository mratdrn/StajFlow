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

public class StudentApplicationService(ApplicationDbContext context) : IStudentApplicationService
{
    private static readonly Expression<Func<InternshipApplication, InternshipApplicationDto>> DtoProjection = application => new(
        application.Id, application.InternshipPostingId, application.InternshipPosting.Title, application.InternshipPosting.CompanyProfile.Name,
        application.InternshipPosting.Location, application.InternshipPosting.WorkArrangement.ToString(), application.InternshipPosting.Status.ToString(),
        application.ResumeId, application.Resume.OriginalFileName, application.CoverLetter, application.Status.ToString(),
        application.AppliedAt, application.StatusUpdatedAt, application.InternshipPosting.ApplicationDeadline);

    public async Task<IReadOnlyList<InternshipApplicationDto>> ListAsync(string userId, CancellationToken cancellationToken) =>
        await BuildListQuery(userId).ToListAsync(cancellationToken);

    public async Task<StudentApplicationPageDto> ListPageAsync(
        string userId, int page, string? status, CancellationToken cancellationToken)
    {
        var counts = await BuildApplicationQuery(userId).GroupBy(application => application.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() }).ToListAsync(cancellationToken);
        var statusCounts = counts.ToDictionary(group => group.Status.ToString(), group => group.Count);
        statusCounts["All"] = counts.Sum(group => group.Count);
        var result = await BuildListQuery(userId, status).ToPageAsync(page, 6, cancellationToken);
        return new(result.Items, result.Page, result.PageSize, result.TotalCount, statusCounts);
    }

    private IQueryable<InternshipApplication> BuildApplicationQuery(string userId) =>
        context.Applications.AsNoTracking()
            .Where(application => application.StudentProfile.IdentityUserId == userId && application.Status != ApplicationStatus.Withdrawn);

    private IQueryable<InternshipApplicationDto> BuildListQuery(string userId, string? status = null)
    {
        var query = BuildApplicationQuery(userId);
        if (status is not null)
        {
            var selectedStatus = Enum.Parse<ApplicationStatus>(status);
            query = query.Where(application => application.Status == selectedStatus);
        }
        return query.OrderByDescending(application => application.AppliedAt).ThenByDescending(application => application.Id)
            .Select(DtoProjection);
    }

    public Task<InternshipApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken) =>
        context.Applications.AsNoTracking()
            .Where(application => application.Id == id && application.StudentProfile.IdentityUserId == userId && application.Status != ApplicationStatus.Withdrawn)
            .Select(DtoProjection).SingleOrDefaultAsync(cancellationToken);

    public Task<InternshipPostingDetailDto?> GetPostingAsync(string userId, int id, CancellationToken cancellationToken) =>
        context.Applications.AsNoTracking()
            .Where(application => application.Id == id && application.StudentProfile.IdentityUserId == userId &&
                application.Status != ApplicationStatus.Withdrawn)
            .Select(application => application.InternshipPosting)
            .Select(InternshipPostingService.DetailProjection).SingleOrDefaultAsync(cancellationToken);

    public async Task<ApplicationWithdrawalResult> WithdrawAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var application = await context.Applications.SingleOrDefaultAsync(application =>
            application.Id == id && application.StudentProfile.IdentityUserId == userId, cancellationToken);
        if (application is null) return new(ApplicationWithdrawalError.NotFound);
        if (application.Status is not (ApplicationStatus.Submitted or ApplicationStatus.UnderReview or ApplicationStatus.Withdrawn))
            return new(ApplicationWithdrawalError.InvalidStatus);
        context.Applications.Remove(application);
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
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        var studentId = await context.StudentProfiles.AsNoTracking()
            .Where(student => student.IdentityUserId == userId)
            .Select(student => (int?)student.Id).SingleOrDefaultAsync(cancellationToken);
        if (studentId is null) return new(null, ApplicationCreationError.NotFound);

        if (await context.Applications.AsNoTracking().AnyAsync(application =>
                application.StudentProfileId == studentId && application.InternshipPostingId == request.InternshipPostingId &&
                application.Status != ApplicationStatus.Withdrawn, cancellationToken))
            return new(null, ApplicationCreationError.AlreadyApplied);

        var posting = await context.InternshipPostings.AsNoTracking()
            .Where(posting => posting.Id == request.InternshipPostingId && posting.Status == PostingStatus.Published)
            .Select(posting => new { posting.ApplicationDeadline }).SingleOrDefaultAsync(cancellationToken);
        if (posting is null) return new(null, ApplicationCreationError.NotFound);

        if (!await context.Resumes.AsNoTracking().AnyAsync(resume =>
                resume.Id == request.ResumeId && resume.StudentProfileId == studentId &&
                resume.ContentType == "application/pdf", cancellationToken))
            return new(null, ApplicationCreationError.InvalidResume);

        var now = DateTimeOffset.UtcNow;
        if (posting.ApplicationDeadline.HasValue && posting.ApplicationDeadline.Value <= now)
            return new(null, ApplicationCreationError.DeadlinePassed);

        await context.Applications.Where(application => application.StudentProfileId == studentId &&
                application.InternshipPostingId == request.InternshipPostingId && application.Status == ApplicationStatus.Withdrawn)
            .ExecuteDeleteAsync(cancellationToken);

        var application = new InternshipApplication
        {
            StudentProfileId = studentId.Value, InternshipPostingId = request.InternshipPostingId,
            ResumeId = request.ResumeId,
            CoverLetter = string.IsNullOrWhiteSpace(request.CoverLetter) ? null : request.CoverLetter.Trim(),
            Status = ApplicationStatus.Submitted, AppliedAt = now
        };
        context.Applications.Add(application);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            var result = await GetAsync(userId, application.Id, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(result);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return new(null, ApplicationCreationError.AlreadyApplied);
        }
    }
}
