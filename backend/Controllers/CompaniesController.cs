using System.Security.Claims;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Profiles;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/companies")]
[RequireHttps]
[Authorize(Roles = UserRoles.Company)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CompaniesController(IProfileService service) : ControllerBase
{
    [HttpPut("me")]
    public async Task<ActionResult<CompanyProfileDto>> Update(UpdateCompanyProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await service.UpdateCompanyAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("me")]
    public async Task<ActionResult<CompanyProfileDto>> Me(CancellationToken cancellationToken)
    {
        var profile = await service.GetCompanyAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}

