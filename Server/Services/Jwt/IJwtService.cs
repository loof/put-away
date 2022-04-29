using System.IdentityModel.Tokens.Jwt;
using PutAway.Shared.Entities;
using PutAway.Shared.Entities.Dto;

namespace PutAway.Server.Services.Jwt;

public interface IJwtService
{
    string CreateToken(User user);

    string GenerateRefreshTokenString();

    JwtSecurityToken GetJwtToken(string expiredToken);
}