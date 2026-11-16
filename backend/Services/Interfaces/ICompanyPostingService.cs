using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.PostingManagement;

namespace StajFlow.Api.Services.Interfaces;

public interface ICompanyPostingService
{
    Task<IReadOnlyList<InternshipPostingListDto>> ListAsync(string userId, bool? archived, CancellationToken cancellationToken);
    Task<CompanyPostingDto?> GetAsync(string userId, int id, CancellationToken cancellationToken);
    Task<PostingManagementResult> CreateAsync(string userId, CreatePostingRequest request, CancellationToken cancellationToken);
    Task<PostingManagementResult> UpdateAsync(string userId, int id, UpdatePostingRequest request, CancellationToken cancellationToken);
}
