using System.IdentityModel.Tokens.Jwt;
using PutAway.Shared.Entities;
using PutAway.Shared.Entities.Dto;

namespace PutAway.Server.Services.Jwt;

public interface IJwtService
{

    Task<UserLoginResponseDto> GetTokenAsync(UserLoginRequestDto authRequest,string ipAddress);

    Task<UserLoginResponseDto> GetRefreshTokenAsync(string ipAddress, User user);

    JwtSecurityToken GetJwtToken(string expiredToken);

    Task<bool> IsTokenValid(string accessToken, string ipAddress);

}