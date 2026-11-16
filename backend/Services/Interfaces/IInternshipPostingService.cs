using StajFlow.Api.DTOs;

namespace StajFlow.Api.Services.Interfaces;

public interface IInternshipPostingService
{
    Task<IReadOnlyList<InternshipPostingListDto>> ListAsync(PostingSearchRequest search, CancellationToken cancellationToken = default);
    Task<InternshipPostingDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}

