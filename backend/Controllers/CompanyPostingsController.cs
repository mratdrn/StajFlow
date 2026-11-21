using System.Security.Claims;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.PostingManagement;
using StajFlow.Api.Entities;
using StajFlow.Api.Services;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/companies/me/internship-postings")]
[RequireHttps]
[Authorize(Roles = UserRoles.Company)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class CompanyPostingsController(ICompanyPostingService service) : ControllerBase
{
    private string IdentityUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InternshipPostingListDto>>> List([FromQuery(Name = "archived")] bool? archived, CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(IdentityUserId, archived, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompanyPostingDto>> Get(int id, CancellationToken cancellationToken)
    {
        var posting = await service.GetAsync(IdentityUserId, id, cancellationToken);
        return posting is null ? NotFound() : Ok(posting);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreatePostingRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(IdentityUserId, request, cancellationToken);
        return result.Error.HasValue ? ReturnError(result.Error.Value) :
            CreatedAtAction(nameof(Get), new { id = result.Posting!.Id }, result.Posting);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePostingRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(IdentityUserId, id, request, cancellationToken);
        return result.Error.HasValue ? ReturnError(result.Error.Value) : Ok(result.Posting);
    }

    private IActionResult ReturnError(PostingManagementError error) => error switch
    {
        PostingManagementError.NotFound => NotFound(),
        PostingManagementError.VersionConflict => Conflict(new { message = "İlan başka bir işlemde güncellendi. Güncel sürümü yükleyip tekrar deneyin." }),
        PostingManagementError.InvalidDeadline => ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            { ["applicationDeadline"] = ["Son başvuru için bugün veya sonraki bir tarih ve henüz geçmemiş bir saat seçin."] })
            { Title = "Son başvuru tarihini kontrol edin." }),
        _ => ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            { ["requirements"] = ["Nitelik listesinde bu ilana ait olmayan bir kayıt var."] }) { Title = "Bilgileri kontrol edin." })
    };
}
