using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
namespace Karnel_Travel.Data;
// Add profile data for application users by adding properties to the ApplicationUser class
public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [MaxLength(100)]
    public string? FullName { get; set; }

    [PersonalData]
    [MaxLength(100)]
    public string? Country { get; set; }
}
