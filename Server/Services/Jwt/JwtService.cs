using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PutAway.Server.Data;
using PutAway.Shared.Entities;
using PutAway.Shared.Entities.Dto;

namespace PutAway.Server.Services.Jwt;

public class JwtService : IJwtService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private const int TokenTimeoutSeconds = 180;
    
    public JwtService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.EmailAddress)
        };

        foreach (Role userRole in user.Roles)
        {
            claims.Add(new(ClaimTypes.Role, userRole.Name));
        }

        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("Authentication:JWT:Token").Value));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddSeconds(TokenTimeoutSeconds),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    public string GenerateRefreshTokenString()
    {
        var byteArray = new byte[64];
        using (var cryproProvider = new RNGCryptoServiceProvider())
        {
            cryproProvider.GetBytes(byteArray);

            return Convert.ToBase64String(byteArray);
        }
    }
    
    public JwtSecurityToken GetJwtToken(string expiredToken)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ReadJwtToken(expiredToken);
    }
}