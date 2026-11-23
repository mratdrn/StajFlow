using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;
using StajFlow.Api.Services;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/internship-postings")]
public class InternshipPostingsController(IInternshipPostingService service, CompatibilityService compatibility) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<InternshipPostingListDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<PagedResult<InternshipPostingListDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> List(
        [FromQuery] PostingSearchRequest search,
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası en az 1 olmalıdır.")] int? page,
        CancellationToken cancellationToken)
    {
        if (page.HasValue) return Ok(await service.ListPageAsync(search, page.Value, cancellationToken));
        return Ok(await service.ListAsync(search, cancellationToken));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<InternshipPostingDetailDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<InternshipPostingDetailDto>> GetById(
        int id, CancellationToken cancellationToken)
    {
        var posting = await service.GetByIdAsync(id, cancellationToken);
        if (posting is null)
        {
            return NotFound();
        }

        return Ok(posting);
    }

    [HttpGet("{id:int}/compatibility")]
    [Authorize(Roles = UserRoles.Student)]
    [RequireHttps]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<ActionResult<CompatibilityDto>> GetCompatibility(int id,
        [FromQuery, Range(1, int.MaxValue)] int? resumeId, CancellationToken cancellationToken)
    {
        var result = await compatibility.GetAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, id, resumeId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}

