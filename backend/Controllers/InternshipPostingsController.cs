using System.ComponentModel.DataAnnotations;
using StajFlow.Api.DTOs;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/internship-postings")]
public class InternshipPostingsController(IInternshipPostingService service) : ControllerBase
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
}

