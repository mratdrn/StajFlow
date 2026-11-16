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
    public async Task<ActionResult<IReadOnlyList<InternshipPostingListDto>>> List(
        [FromQuery] PostingSearchRequest search, CancellationToken cancellationToken)
    {
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

