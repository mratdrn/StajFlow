using StajFlow.Api.Data;
using StajFlow.Api.Entities;
using StajFlow.Api.Services;
using StajFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StajFlow.Api.Configurations;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("tr-TR");
CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("tr-TR");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection bağlantı ayarı zorunludur.")));

builder.Services.AddScoped<IInternshipPostingService, InternshipPostingService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<ICompanyPostingService, CompanyPostingService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IStudentApplicationService, StudentApplicationService>();
builder.Services.AddScoped<ICompanyApplicationService, CompanyApplicationService>();
builder.Services.AddOptions<ResumeStorageOptions>()
    .BindConfiguration(ResumeStorageOptions.SectionName)
    .Validate(settings => settings.MaxFileSizeMb is >= 1 and <= 20, "Özgeçmiş dosya sınırı 1–20 MB arasında yapılandırılmalıdır.")
    .ValidateOnStart();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = string.Empty;
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<TurkishIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "__Host-StajFlow.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
builder.Services.AddAuthorization();
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.Cookie.Name = "__Host-StajFlow.Csrf";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Path = "/";
});
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
    options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => "İstek gövdesi zorunludur.");
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Bu alan boş bırakılamaz.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(_ => "Gönderilen değer geçersiz.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, _) => "Gönderilen değer geçersiz.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => "Gönderilen değer geçersiz.");
    options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(_ => "Gönderilen değer geçersiz.");
    options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => "Gönderilen değer geçersiz.");
})
    .AddJsonOptions(options => options.AllowInputFormatterExceptionMessages = false)
    .ConfigureApiBehaviorOptions(options =>
    {
        foreach (var (status, title) in new[] { (400, "Geçersiz istek"), (401, "Giriş yapmanız gerekiyor"),
                     (403, "Erişim yetkiniz yok"), (404, "Kayıt bulunamadı"), (500, "Sunucu hatası") })
            options.ClientErrorMapping[status].Title = title;

        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var errors = actionContext.ModelState.Where(field => field.Value?.Errors.Count > 0)
                .ToDictionary(field => field.Key, field => field.Value!.Errors.Select(error =>
                    field.Key.StartsWith('$') || error.Exception is not null || string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "Gönderilen bilgiler geçersiz." : error.ErrorMessage).ToArray());
            return new BadRequestObjectResult(new ValidationProblemDetails(errors) { Title = "Bilgileri kontrol edin." });
        };
    });
builder.Services.AddOpenApi();

const string frontendDevelopmentPolicy = "FrontendDevelopment";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
        options.AddPolicy(frontendDevelopmentPolicy, policy =>
            policy.WithOrigins("https://localhost:5173")
                .WithMethods("GET", "POST", "PUT")
                .WithHeaders("Accept", "Content-Type", "X-CSRF-TOKEN")
                .AllowCredentials()));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();
if (app.Environment.IsDevelopment())
{
    app.UseCors(frontendDevelopmentPolicy);
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
