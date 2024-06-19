using System.ComponentModel.DataAnnotations;

namespace DotNet7_ExpenseTrackerApi.Models.RequestModels.User;
public class RegisterRequestModel
{
    public string UserName { get; set; } = null!;
    [EmailAddress]
    public string Email { get; set; } = null!;
    public int UserRole { get; set; }
    [MaxLength(16)]
    [MinLength(8)]
    public string Password { get; set; } = null!;
    public string DOB { get; set; } = null!;
    public string Gender { get; set; } = null!;

}
