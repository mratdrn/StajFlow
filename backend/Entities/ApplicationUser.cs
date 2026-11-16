using Microsoft.AspNetCore.Identity;

namespace StajFlow.Api.Entities;

public class ApplicationUser : IdentityUser
{
    public StudentProfile? StudentProfile { get; set; }
    public CompanyProfile? CompanyProfile { get; set; }
}

