using System.ComponentModel.DataAnnotations;

namespace PutAway.Shared.Entities.Dto;

public class UserRegisterRequestDto
{
    [Required]
    public string EmailAddress { get; set; }
    
    [Required]
    public string Password { get; set; }
    
}