using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.PostingManagement;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class CompanyPostingService(ApplicationDbContext context) : ICompanyPostingService
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

        var posting = new InternshipPosting { CompanyProfileId = companyId.Value, Title = request.Title.Trim(), Description = request.Description.Trim() };
        ApplyContent(posting, request);
        foreach (var requirement in request.Requirements)
            posting.Requirements.Add(new InternshipRequirement { Description = requirement.Description.Trim(), Type = Enum.Parse<RequirementType>(requirement.Type) });
        context.InternshipPostings.Add(posting);
        await context.SaveChangesAsync(cancellationToken);
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

        ApplyContent(posting, request);
        var retainedIds = request.Requirements.Where(requirement => requirement.Id.HasValue).Select(requirement => requirement.Id!.Value).ToHashSet();
        foreach (var silinen in posting.Requirements.Where(requirement => !retainedIds.Contains(requirement.Id)).ToList())
        {
            context.InternshipRequirements.Remove(silinen);
            posting.Requirements.Remove(silinen);
        }
        foreach (var gelen in request.Requirements)
        {
            if (gelen.Id.HasValue)
            {
                existing[gelen.Id.Value].Description = gelen.Description.Trim();
                existing[gelen.Id.Value].Type = Enum.Parse<RequirementType>(gelen.Type);
            }
            else posting.Requirements.Add(new InternshipRequirement { Description = gelen.Description.Trim(), Type = Enum.Parse<RequirementType>(gelen.Type) });
        }
        context.Entry(posting).Property(posting => posting.Title).IsModified = true;
        context.Entry(posting).Property(posting => posting.RowVersion).OriginalValue = version;
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return new(CreateDto(posting));
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(null, PostingManagementError.VersionConflict);
        }
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

    private static CompanyPostingDto CreateDto(InternshipPosting posting) => new(posting.Id, posting.Title, posting.Description,
        posting.Location, posting.WorkArrangement.ToString(), posting.Status.ToString(), posting.ApplicationDeadline, posting.StartDate,
        posting.EndDate, posting.CreatedAt,
        posting.Requirements.OrderBy(requirement => requirement.Id)
            .Select(requirement => new InternshipRequirementDto(requirement.Id, requirement.Description, requirement.Type.ToString())).ToList(),
        Convert.ToBase64String(posting.RowVersion), posting.ShortDescription);
}
