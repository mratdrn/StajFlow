using System.Data;
using System.Linq.Expressions;
using StajFlow.Api.Data;
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
        application.InternshipPosting.Location, application.InternshipPosting.IsRemote, application.InternshipPosting.Status.ToString(),
        application.ResumeId, application.Resume.OriginalFileName, application.CoverLetter, application.Status.ToString(),
        application.AppliedAt, application.StatusUpdatedAt, application.InternshipPosting.ApplicationDeadline);

    public async Task<IReadOnlyList<InternshipApplicationDto>> ListAsync(string userId, CancellationToken cancellationToken) =>
        await context.Applications.AsNoTracking()
            .Where(application => application.StudentProfile.IdentityUserId == userId)
            .OrderByDescending(application => application.AppliedAt).ThenByDescending(application => application.Id)
            .Select(DtoProjection).ToListAsync(cancellationToken);

    public Task<InternshipApplicationDto?> GetAsync(string userId, int id, CancellationToken cancellationToken) =>
        context.Applications.AsNoTracking()
            .Where(application => application.Id == id && application.StudentProfile.IdentityUserId == userId)
            .Select(DtoProjection).SingleOrDefaultAsync(cancellationToken);

    public async Task<ApplicationCreationResult> CreateAsync(string userId, CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead, cancellationToken);
        var studentId = await context.StudentProfiles.AsNoTracking()
            .Where(student => student.IdentityUserId == userId)
            .Select(student => (int?)student.Id).SingleOrDefaultAsync(cancellationToken);
        if (studentId is null) return new(null, ApplicationCreationError.NotFound);

        if (await context.Applications.AsNoTracking().AnyAsync(application =>
                application.StudentProfileId == studentId && application.InternshipPostingId == request.InternshipPostingId, cancellationToken))
            return new(null, ApplicationCreationError.AlreadyApplied);

        var posting = await context.InternshipPostings.AsNoTracking()
            .Where(posting => posting.Id == request.InternshipPostingId && posting.Status == PostingStatus.Published)
            .Select(posting => new { posting.ApplicationDeadline }).SingleOrDefaultAsync(cancellationToken);
        if (posting is null) return new(null, ApplicationCreationError.NotFound);

        if (!await context.Resumes.AsNoTracking().AnyAsync(resume =>
                resume.Id == request.ResumeId && resume.StudentProfileId == studentId, cancellationToken))
            return new(null, ApplicationCreationError.InvalidResume);

        var now = DateTimeOffset.UtcNow;
        if (posting.ApplicationDeadline.HasValue && posting.ApplicationDeadline.Value <= now)
            return new(null, ApplicationCreationError.DeadlinePassed);

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
