using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using StajFlow.Api.Configurations;
using StajFlow.Api.DTOs;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/students/me/resumes")]
[RequireHttps]
[Authorize(Roles = UserRoles.Student)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class ResumesController(IResumeService service, IOptions<ResumeStorageOptions> options) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ResumeListDto>> List(CancellationToken cancellationToken) =>
        Ok(new ResumeListDto(options.Value.MaxFileSizeBytes,
            await service.ListAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, cancellationToken)));

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 6 * 1024 * 1024)]
    public async Task<ActionResult<ResumeDto>> Upload([FromForm] UploadResumeRequest request, CancellationToken cancellationToken)
    {
        var result = await service.UploadAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, request.File, cancellationToken);
        if (result.ProfileNotFound) return NotFound();
        if (result.Error is not null) return BadRequest(new { message = result.Error });
        return CreatedAtAction(nameof(ViewFile), new { id = result.Resume!.Id }, result.Resume);
    }

    [HttpGet("{id:int}/file")]
    public async Task<IActionResult> ViewFile(int id, CancellationToken cancellationToken)
    {
        var file = await service.GetFileAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, id, cancellationToken);
        if (file is null) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        Response.Headers.ContentSecurityPolicy = "sandbox; default-src 'none'; frame-ancestors https://localhost:5173";
        return File(file.Content, "application/pdf", file.FileName);
    }
}

public class UploadResumeRequest
{
    [Required(ErrorMessage = "PDF dosyası seçin.")]
    [FromForm(Name = "file")]
    public IFormFile File { get; set; } = null!;
}
