using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using StajFlow.Api.DTOs.Applications;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/companies/me/applications")]
[RequireHttps]
[Authorize(Roles = UserRoles.Company)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CompanyApplicationsController(ICompanyApplicationService service, IResumeService resumeService) : ControllerBase
{
    private string IdentityUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CompanyApplicationDto>>> List(
        [FromQuery(Name = "internshipPostingId"), Range(1, int.MaxValue, ErrorMessage = "Geçerli bir ilan seçin.")] int? postingId,
        CancellationToken cancellationToken) => Ok(await service.ListAsync(IdentityUserId, postingId, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyApplicationDto>> Get(int id, CancellationToken cancellationToken)
    {
        var application = await service.GetAsync(IdentityUserId, id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateApplicationStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateStatusAsync(IdentityUserId, id, request, cancellationToken);
        return result.Error switch
        {
            ApplicationStatusError.NotFound => NotFound(),
            ApplicationStatusError.VersionConflict => Conflict(new { message = "Başvuru başka bir işlemde güncellendi. Güncel başvuruyu yükleyip tekrar deneyin." }),
            ApplicationStatusError.Withdrawn => Conflict(new { message = "Geri çekilmiş başvurunun durumu değiştirilemez." }),
            _ => Ok(result.Application)
        };
    }

    [HttpGet("{id:int}/resume")]
    public async Task<IActionResult> ViewResume(int id, CancellationToken cancellationToken)
    {
        var file = await resumeService.GetCompanyApplicationFileAsync(IdentityUserId, id, cancellationToken);
        if (file is null) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'; frame-ancestors 'none'";
        return File(file.Content, "application/pdf", file.FileName);
    }
}
