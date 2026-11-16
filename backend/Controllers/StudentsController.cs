using System.Security.Claims;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Profiles;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/students")]
[RequireHttps]
[Authorize(Roles = UserRoles.Student)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class StudentsController(IProfileService service) : ControllerBase
{
    [HttpPut("me")]
    public async Task<ActionResult<StudentProfileDto>> Update(UpdateStudentProfileRequest request, CancellationToken cancellationToken)
    {
        var profile = await service.UpdateStudentAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }

    [HttpGet("me")]
    public async Task<ActionResult<StudentProfileDto>> Me(CancellationToken cancellationToken)
    {
        var profile = await service.GetStudentAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, cancellationToken);
        return profile is null ? NotFound() : Ok(profile);
    }
}

