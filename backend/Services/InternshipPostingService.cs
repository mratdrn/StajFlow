using System.Linq.Expressions;
using StajFlow.Api.Data;
using StajFlow.Api.DTOs;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Services;

public class InternshipPostingService(ApplicationDbContext context) : IInternshipPostingService
{
    internal static readonly Expression<Func<InternshipPosting, InternshipPostingDetailDto>> DetailProjection =
        posting => new InternshipPostingDetailDto(
            posting.Id,
            posting.Title,
            posting.Description,
            posting.CompanyProfileId,
            posting.CompanyProfile.Name,
            posting.Location,
            posting.WorkArrangement.ToString(),
            posting.Status.ToString(),
            posting.ApplicationDeadline,
            posting.StartDate,
            posting.EndDate,
            posting.CreatedAt,
            new List<InternshipRequirementDto>(), posting.ShortDescription,
            posting.CompanyProfile.LogoVersion.HasValue ? "/api/companies/" + posting.CompanyProfileId + "/logo?v=" + posting.CompanyProfile.LogoVersion.Value.ToString().ToLower() : null);

    public async Task<IReadOnlyList<InternshipPostingListDto>> ListAsync(
        PostingSearchRequest search, CancellationToken cancellationToken = default) =>
        await BuildListQuery(search).ToListAsync(cancellationToken);

    public Task<PagedResult<InternshipPostingListDto>> ListPageAsync(
        PostingSearchRequest search, int page, CancellationToken cancellationToken) =>
        BuildListQuery(search).ToPageAsync(page, 9, cancellationToken);

    private IQueryable<InternshipPostingListDto> BuildListQuery(PostingSearchRequest search)
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
        if (search.WorkArrangement is not null)
        {
            var arrangement = Enum.Parse<WorkArrangement>(search.WorkArrangement);
            query = query.Where(posting => posting.WorkArrangement == arrangement);
        }

        return query
            .OrderByDescending(posting => posting.CreatedAt)
            .ThenByDescending(posting => posting.Id)
            .Select(posting => new InternshipPostingListDto(
                posting.Id,
                posting.Title,
                posting.CompanyProfile.Name,
                posting.Location,
                posting.WorkArrangement.ToString(),
                posting.Status.ToString(),
                posting.ApplicationDeadline,
                posting.StartDate,
                posting.EndDate,
                posting.CreatedAt, posting.ShortDescription,
                posting.CompanyProfile.LogoVersion.HasValue ? "/api/companies/" + posting.CompanyProfileId + "/logo?v=" + posting.CompanyProfile.LogoVersion.Value.ToString().ToLower() : null));
    }

    public async Task<InternshipPostingDetailDto?> GetByIdAsync(
        int id, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        return await context.InternshipPostings
            .AsNoTracking()
            .Where(posting => posting.Id == id && posting.Status == PostingStatus.Published &&
                (posting.ApplicationDeadline == null || posting.ApplicationDeadline > now))
            .Select(DetailProjection)
            .SingleOrDefaultAsync(cancellationToken);
    }
}

