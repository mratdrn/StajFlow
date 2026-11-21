namespace StajFlow.Api.DTOs.Applications;

public record StudentApplicationPageDto(
    IReadOnlyList<InternshipApplicationDto> Items, int Page, int PageSize, int TotalCount,
    IReadOnlyDictionary<string, int> StatusCounts)
    : PagedResult<InternshipApplicationDto>(Items, Page, PageSize, TotalCount);
