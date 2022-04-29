namespace PutAway.Shared.Entities.Dto;

public class UserLoginResponseDto
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }

    public bool IsSuccess { get; set; }

    public string Reason { get; set; }
}