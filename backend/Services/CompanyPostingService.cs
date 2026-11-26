using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.PostingManagement;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class CompanyPostingService(ApplicationDbContext context, PostingAnalysisService analyses) : ICompanyPostingService
{
    public async Task<IReadOnlyList<InternshipPostingListDto>> ListAsync(string userId, bool? archived, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var query = context.InternshipPostings.AsNoTracking().Where(posting => posting.CompanyProfile.IdentityUserId == userId);
        if (archived == true)
            query = query.Where(posting => posting.Status == PostingStatus.Closed || posting.ApplicationDeadline <= now);
        else if (archived == false)
            query = query.Where(posting => posting.Status != PostingStatus.Closed && (posting.ApplicationDeadline == null || posting.ApplicationDeadline > now));
        return await query
            .OrderByDescending(posting => posting.CreatedAt).ThenByDescending(posting => posting.Id)
            .Select(posting => new InternshipPostingListDto(posting.Id, posting.Title, posting.CompanyProfile.Name,
                posting.Location, posting.WorkArrangement.ToString(), posting.Status.ToString(), posting.ApplicationDeadline,
                posting.StartDate, posting.EndDate, posting.CreatedAt, posting.ShortDescription))
            .ToListAsync(cancellationToken);
    }

    public async Task<CompanyPostingDto?> GetAsync(string userId, int id, CancellationToken cancellationToken)
    {
        var posting = await context.InternshipPostings.AsNoTracking().Include(posting => posting.Requirements)
            .SingleOrDefaultAsync(posting => posting.Id == id && posting.CompanyProfile.IdentityUserId == userId, cancellationToken);
        return posting is null ? null : CreateDto(posting);
    }

    public async Task<PostingManagementResult> CreateAsync(string userId, CreatePostingRequest request, CancellationToken cancellationToken)
    {
        var companyId = await context.CompanyProfiles.Where(company => company.IdentityUserId == userId)
            .Select(company => (int?)company.Id).SingleOrDefaultAsync(cancellationToken);
        if (companyId is null) return new(null, PostingManagementError.NotFound);
        if (request.ApplicationDeadline <= DateTimeOffset.UtcNow)
            return new(null, PostingManagementError.InvalidDeadline);
        var description = ComposeDescription(request);
        if (description.Length > 10000) return new(null, PostingManagementError.InvalidDescription);

        var posting = new InternshipPosting { CompanyProfileId = companyId.Value, Title = request.Title.Trim(), Description = request.Description.Trim() };
        ApplyContent(posting, request);
        posting.Description = description;
        context.InternshipPostings.Add(posting);
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        await QueuePublishedAnalysisAsync(posting, retryFailed: false, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new(CreateDto(posting));
    }

    public async Task<PostingManagementResult> UpdateAsync(string userId, int id, UpdatePostingRequest request, CancellationToken cancellationToken)
    {
        var posting = await context.InternshipPostings.Include(posting => posting.Requirements)
            .SingleOrDefaultAsync(posting => posting.Id == id && posting.CompanyProfile.IdentityUserId == userId, cancellationToken);
        if (posting is null) return new(null, PostingManagementError.NotFound);

        var version = Convert.FromBase64String(request.RowVersion);
        if (!posting.RowVersion.AsSpan().SequenceEqual(version))
            return new(null, PostingManagementError.VersionConflict);
        if (request.ApplicationDeadline <= DateTimeOffset.UtcNow &&
            (request.ApplicationDeadline != posting.ApplicationDeadline ||
             (request.Status == "Published" && posting.Status != PostingStatus.Published)))
            return new(null, PostingManagementError.InvalidDeadline);
        var existing = posting.Requirements.ToDictionary(requirement => requirement.Id);
        if (request.Requirements.Any(requirement => requirement.Id.HasValue && !existing.ContainsKey(requirement.Id.Value)))
            return new(null, PostingManagementError.InvalidRequirement);
        var description = ComposeDescription(request);
        if (description.Length > 10000) return new(null, PostingManagementError.InvalidDescription);

        var newlyPublished = posting.Status != PostingStatus.Published && request.Status == nameof(PostingStatus.Published);
        ApplyContent(posting, request);
        posting.Description = description;
        context.Entry(posting).Property(posting => posting.Title).IsModified = true;
        context.Entry(posting).Property(posting => posting.RowVersion).OriginalValue = version;
        try
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            await QueuePublishedAnalysisAsync(posting, newlyPublished, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new(CreateDto(posting));
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(null, PostingManagementError.VersionConflict);
        }
        catch (DbUpdateException error) when (error.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        {
            return new(null, PostingManagementError.VersionConflict);
        }
        catch (SqlException error) when (error.Number == 1205)
        {
            return new(null, PostingManagementError.VersionConflict);
        }
    }

    private async Task QueuePublishedAnalysisAsync(InternshipPosting posting, bool retryFailed, CancellationToken cancellationToken)
    {
        if (posting.Status != PostingStatus.Published || posting.ApplicationDeadline <= DateTimeOffset.UtcNow) return;
        await analyses.QueueAsync(posting, retryFailed, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static void ApplyContent(InternshipPosting posting, PostingContentRequest request)
    {
        posting.Title = request.Title.Trim();
        posting.ShortDescription = string.IsNullOrWhiteSpace(request.ShortDescription) ? null : request.ShortDescription.Trim();
        posting.Description = request.Description.Trim();
        posting.Location = string.IsNullOrWhiteSpace(request.Location) ? null : request.Location.Trim();
        posting.WorkArrangement = Enum.Parse<WorkArrangement>(request.WorkArrangement);
        posting.Status = Enum.Parse<PostingStatus>(request.Status);
        posting.ApplicationDeadline = request.ApplicationDeadline;
        posting.StartDate = request.StartDate;
        posting.EndDate = request.EndDate;
    }

    private static string ComposeDescription(PostingContentRequest request) => PostingDescription.Compose(request.Description,
        request.Requirements.Select(item => (item.Description, Enum.Parse<RequirementType>(item.Type))));

    private static CompanyPostingDto CreateDto(InternshipPosting posting) => new(posting.Id, posting.Title, posting.Description,
        posting.Location, posting.WorkArrangement.ToString(), posting.Status.ToString(), posting.ApplicationDeadline, posting.StartDate,
        posting.EndDate, posting.CreatedAt, [],
        Convert.ToBase64String(posting.RowVersion), posting.ShortDescription);
}
