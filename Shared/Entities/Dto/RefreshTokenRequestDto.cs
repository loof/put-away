using System.ComponentModel.DataAnnotations;

namespace PutAway.Shared.Entities.Dto;

public class RefreshTokenRequestDto
{
    [Required]
    public string ExpiredToken { get; set; }
    
    [Required]
    public string RefreshToken { get; set; }
}