using System.ComponentModel.DataAnnotations;

namespace PutAway.Shared.Entities.Dto;

public class UserRegisterDto
{
    [Required]
    public string EmailAddress { get; set; }
    
    [Required]
    public string Password { get; set; }
    
}