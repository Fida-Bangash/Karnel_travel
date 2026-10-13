using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace Karnel_Travel.Data;
// Application user with extra profile fields.
public class ApplicationUser : IdentityUser
{
    // Full name (personal data).
    [PersonalData]
    [MaxLength(100)]
    public string? FullName { get; set; }

    // Country (personal data).
    [PersonalData]
    [MaxLength(100)]
    public string? Country { get; set; }
}
