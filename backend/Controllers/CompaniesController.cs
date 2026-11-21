using System.Security.Claims;
using StajFlow.Api.DTOs;
using StajFlow.Api.DTOs.Auth;
using StajFlow.Api.Services;
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
public class CompaniesController(IProfileService service, IAuthenticationService authenticationService, ICompanyLogoService logoService) : ControllerBase
{
    [HttpPut("me/logo")]
    [RequestSizeLimit(CompanyLogoService.MaxFileSize + 65536)]
    [RequestFormLimits(MultipartBodyLengthLimit = CompanyLogoService.MaxFileSize + 65536)]
    public async Task<IActionResult> UploadLogo(
        [FromForm, System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Logo dosyasını seçin.")] IFormFile file,
        CancellationToken cancellationToken)
    {
        var result = await logoService.UploadAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)!, file, cancellationToken);
        if (result.ProfileNotFound) return NotFound();
        if (result.Error is not null)
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]> { ["file"] = [result.Error] })
                { Title = "Logo yüklenemedi." });
        return Ok(new { logoUrl = result.LogoUrl });
    }

    [AllowAnonymous]
    [HttpGet("{id:int}/logo")]
    public async Task<IActionResult> Logo(int id, CancellationToken cancellationToken)
    {
        var logo = await logoService.GetAsync(id, cancellationToken);
        if (logo is null) return NotFound();
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(logo.Data, logo.ContentType);
    }

    [HttpDelete("me")]
    public async Task<IActionResult> Delete(DeleteAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.DeleteCompanyAccountAsync(User, request.Password, cancellationToken);
        return result switch
        {
            CompanyAccountDeletionResult.Succeeded => NoContent(),
            CompanyAccountDeletionResult.NotFound => NotFound(),
            CompanyAccountDeletionResult.InvalidPassword => ValidationProblem(new ValidationProblemDetails(
                new Dictionary<string, string[]> { ["password"] = ["Parola hatalı veya hesap geçici olarak kilitli."] })
                { Title = "Hesap silinemedi." }),
            _ => Conflict(new { message = "Hesap başka bir işlemde değişti. Sayfayı yenileyip tekrar deneyin." })
        };
    }

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

