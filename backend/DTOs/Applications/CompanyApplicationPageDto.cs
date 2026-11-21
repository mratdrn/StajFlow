namespace StajFlow.Api.DTOs.Applications;

public record CompanyApplicationPageDto(
    IReadOnlyList<CompanyApplicationDto> Items, int Page, int PageSize, int TotalCount,
    IReadOnlyDictionary<string, int> StatusCounts)
    : PagedResult<CompanyApplicationDto>(Items, Page, PageSize, TotalCount);
