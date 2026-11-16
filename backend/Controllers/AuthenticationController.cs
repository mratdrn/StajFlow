using StajFlow.Api.DTOs.Auth;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StajFlow.Api.Controllers;

[ApiController]
[Route("api/auth")]
[RequireHttps]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class AuthenticationController(IAuthenticationService service) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("csrf")]
    public IActionResult GetCsrfToken([FromServices] IAntiforgery antiforgery)
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new { requestToken = tokens.RequestToken });
    }

    [AllowAnonymous]
    [HttpPost("register/student")]
    public async Task<IActionResult> RegisterStudent(StudentRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RegisterStudentAsync(request, cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, new { message = "Hesap oluşturuldu. Devam etmek için giriş yapın." })
            : ValidationProblem(new ValidationProblemDetails(result.Errors) { Title = "Bilgileri kontrol edin." });
    }

    [AllowAnonymous]
    [HttpPost("register/company")]
    public async Task<IActionResult> RegisterCompany(CompanyRegistrationRequest request, CancellationToken cancellationToken)
    {
        var result = await service.RegisterCompanyAsync(request, cancellationToken);
        return result.Succeeded
            ? StatusCode(StatusCodes.Status201Created, new { message = "Hesap oluşturuldu. Devam etmek için giriş yapın." })
            : ValidationProblem(new ValidationProblemDetails(result.Errors) { Title = "Bilgileri kontrol edin." });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        return await service.LoginAsync(request)
            ? Ok(new { message = "Başarıyla giriş yaptınız." })
            : Unauthorized(new { message = "E-posta veya parola hatalı ya da hesap geçici olarak kilitli." });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        return await service.LogoutAsync(User)
            ? NoContent()
            : Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Oturum kapatılamadı. Lütfen tekrar deneyin.");
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me(CancellationToken cancellationToken)
    {
        var user = await service.GetCurrentUserAsync(User, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}

