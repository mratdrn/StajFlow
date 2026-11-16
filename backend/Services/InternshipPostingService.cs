using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class InternshipPostingService(ApplicationDbContext context) : IInternshipPostingService
{
    public async Task<IReadOnlyList<InternshipPostingListDto>> ListAsync(
        PostingSearchRequest search, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var query = context.InternshipPostings
            .AsNoTracking()
            .Where(posting => posting.Status == PostingStatus.Published && (posting.ApplicationDeadline == null || posting.ApplicationDeadline > now));

        var keyword = search.Keyword?.Trim();
        var location = search.Location?.Trim();
        if (!string.IsNullOrEmpty(keyword))
            query = query.Where(posting => posting.Title.Contains(keyword) || posting.Description.Contains(keyword) ||
                (posting.ShortDescription != null && posting.ShortDescription.Contains(keyword)) || posting.CompanyProfile.Name.Contains(keyword));
        if (!string.IsNullOrEmpty(location))
            query = query.Where(posting => posting.Location != null && posting.Location.Contains(location));
        if (search.IsRemote.HasValue)
            query = query.Where(posting => posting.IsRemote == search.IsRemote.Value);

        return await query
            .OrderByDescending(posting => posting.CreatedAt)
            .ThenByDescending(posting => posting.Id)
            .Select(posting => new InternshipPostingListDto(
                posting.Id,
                posting.Title,
                posting.CompanyProfile.Name,
                posting.Location,
                posting.IsRemote,
                posting.Status.ToString(),
                posting.ApplicationDeadline,
                posting.StartDate,
                posting.EndDate,
                posting.CreatedAt, posting.ShortDescription))
            .ToListAsync(cancellationToken);
    }

    public async Task<InternshipPostingDetailDto?> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await context.InternshipPostings
            .AsNoTracking()
            .Where(posting => posting.Id == id && posting.Status == PostingStatus.Published &&
                (posting.ApplicationDeadline == null || posting.ApplicationDeadline > now))
            .Select(posting => new InternshipPostingDetailDto(
                posting.Id,
                posting.Title,
                posting.Description,
                posting.CompanyProfileId,
                posting.CompanyProfile.Name,
                posting.Location,
                posting.IsRemote,
                posting.Status.ToString(),
                posting.ApplicationDeadline,
                posting.StartDate,
                posting.EndDate,
                posting.CreatedAt,
                posting.Requirements
                    .OrderBy(requirement => requirement.Type)
                    .ThenBy(requirement => requirement.Id)
                    .Select(requirement => new InternshipRequirementDto(
                        requirement.Id,
                        requirement.Description,
                        requirement.Type.ToString()))
                    .ToList(), posting.ShortDescription))
            .SingleOrDefaultAsync(cancellationToken);
    }
}

