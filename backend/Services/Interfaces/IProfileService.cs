using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Profiles;

namespace StajFlow.Api.Services.Interfaces;

public interface IProfileService
{
    Task<StudentProfileDto?> GetStudentAsync(string userId, CancellationToken cancellationToken);
    Task<CompanyProfileDto?> GetCompanyAsync(string userId, CancellationToken cancellationToken);
    Task<StudentProfileDto?> UpdateStudentAsync(string userId, UpdateStudentProfileRequest request, CancellationToken cancellationToken);
    Task<CompanyProfileDto?> UpdateCompanyAsync(string userId, UpdateCompanyProfileRequest request, CancellationToken cancellationToken);
}

