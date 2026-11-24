using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using StajFlow.Api.DTOs;
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
    public async Task<IActionResult> List(
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası en az 1 olmalıdır.")] int? page,
        [FromQuery, AllowedValues(null, "Submitted", "UnderReview", "Shortlisted", "Accepted", "Rejected", "Withdrawn",
            ErrorMessage = "Başvuru durumu geçersiz.")] string? status,
        CancellationToken cancellationToken)
    {
        if (page.HasValue || status is not null)
            return Ok(await service.ListPageAsync(IdentityUserId, page ?? 1, status, cancellationToken));
        return Ok(await service.ListAsync(IdentityUserId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InternshipApplicationDto>> Get(int id, CancellationToken cancellationToken)
    {
        var application = await service.GetAsync(IdentityUserId, id, cancellationToken);
        return application is null ? NotFound() : Ok(application);
    }

    [HttpGet("{id:int}/posting")]
    public async Task<ActionResult<InternshipPostingDetailDto>> GetPosting(int id, CancellationToken cancellationToken)
    {
        var posting = await service.GetPostingAsync(IdentityUserId, id, cancellationToken);
        return posting is null ? NotFound() : Ok(posting);
    }

    [HttpPost("{id:int}/withdraw")]
    public async Task<IActionResult> Withdraw(int id, CancellationToken cancellationToken)
    {
        var result = await service.WithdrawAsync(IdentityUserId, id, cancellationToken);
        return result.Error switch
        {
            ApplicationWithdrawalError.NotFound => NotFound(),
            ApplicationWithdrawalError.InvalidStatus => Conflict(new { message = "Yalnızca gönderilmiş veya incelenen başvurular geri çekilebilir." }),
            ApplicationWithdrawalError.VersionConflict => Conflict(new { message = "Başvuru durumu değişti. Durumu yenileyip tekrar deneyin." }),
            _ => NoContent()
        };
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(IdentityUserId, request, cancellationToken);
        return result.Error switch
        {
            ApplicationCreationError.NotFound => NotFound(new { message = "İlan bulunamadı veya başvuruya açık değil." }),
            ApplicationCreationError.InvalidResume => BadRequest(new { message = "Kendi hesabınıza ait PDF formatında bir özgeçmiş seçin." }),
            ApplicationCreationError.AlreadyApplied => Conflict(new { message = "Bu ilana zaten başvurdunuz. Başvurularım sayfasından takip edebilirsiniz." }),
            ApplicationCreationError.DeadlinePassed => Conflict(new { message = "Bu ilanın son başvuru tarihi geçti." }),
            ApplicationCreationError.VersionConflict => Conflict(new { message = "Başvuru bilgileri değişti. Sayfayı yenileyip tekrar deneyin." }),
            _ => CreatedAtAction(nameof(Get), new { id = result.Application!.Id }, result.Application)
        };
    }
}
