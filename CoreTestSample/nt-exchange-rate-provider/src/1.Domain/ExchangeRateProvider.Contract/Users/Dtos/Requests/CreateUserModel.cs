using System.ComponentModel.DataAnnotations;

namespace ExchangeRateProvider.Contract.Users.Dtos.Requests;

public class CreateUserModel
{
    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

    [EmailAddress] 
    [Required] 
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.(com|org|net|ir|ac\.ir|co\.ir)$", ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required]
    public string Username { get; set; }

    public string RedirectTo { get; set; }
    public bool IsActive { get; set; }
}