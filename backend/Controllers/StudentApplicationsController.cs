using System.Security.Claims;
using StajFlow.Api.DTOs.Applications;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/students/me/applications")]
[RequireHttps]
[Authorize(Roles = UserRoles.Student)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class StudentApplicationsController(IStudentApplicationService service) : ControllerBase
{
    private string IdentityUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InternshipApplicationDto>>> List(CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(IdentityUserId, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InternshipApplicationDto>> Get(int id, CancellationToken cancellationToken)
    {
        var application = await service.GetAsync(IdentityUserId, id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(IdentityUserId, request, cancellationToken);
        return result.Error switch
        {
            ApplicationCreationError.NotFound => NotFound(new { message = "İlan bulunamadı veya başvuruya açık değil." }),
            ApplicationCreationError.InvalidResume => BadRequest(new { message = "Kendi hesabınıza ait bir özgeçmiş seçin." }),
            ApplicationCreationError.AlreadyApplied => Conflict(new { message = "Bu ilana zaten başvurdunuz. Başvurularım sayfasından takip edebilirsiniz." }),
            ApplicationCreationError.DeadlinePassed => Conflict(new { message = "Bu ilanın son başvuru tarihi geçti." }),
            _ => CreatedAtAction(nameof(Get), new { id = result.Application!.Id }, result.Application)
        };
    }
}
