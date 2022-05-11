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
    private const int TokenTimeoutMinutes = 30;
    private const int RefreshTokenTimeoutDays = 1;
    public JwtService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    public async Task<UserLoginResponseDto> GetRefreshTokenAsync(string ipAddress, User user)
    {
        var refreshToken = GenerateRefreshToken();
        var accessToken = CreateToken(user);
        return await SaveTokenDetails(ipAddress, user, accessToken, refreshToken);
    }
    public async Task<UserLoginResponseDto> GetTokenAsync(UserLoginRequestDto authRequest, string ipAddress)
    {
        User? user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.EmailAddress == authRequest.EmailAddress);
      

        if (user == null ||
            !VerifyPasswordHash(authRequest.Password, user.PasswordHash, user.PasswordSalt))
        {
            return await Task.FromResult<UserLoginResponseDto>(null);
        }

        string tokenString = CreateToken(user);
        string refreshToken = GenerateRefreshToken();
        return await SaveTokenDetails(ipAddress, user, tokenString, refreshToken);
    }

    public JwtSecurityToken GetJwtToken(string expiredToken)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.ReadJwtToken(expiredToken);
    }

    public async Task<bool> IsTokenValid(string accessToken, string ipAddress)
    {
        var isValid = _context.UserRefreshTokens.FirstOrDefaultAsync(urt => urt.Token == accessToken && urt.IpAddress == ipAddress) != null;
        return await Task.FromResult(isValid);
    }

    private string CreateToken(User user)
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

        var token = new JwtSecurityToken(claims: claims, expires: DateTime.UtcNow.AddMinutes(TokenTimeoutMinutes),
            signingCredentials: creds);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }
    private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computeHash.SequenceEqual(passwordHash);
        }
    }


    private string GenerateRefreshToken()
    {
        var byteArray = new byte[64];
        using (var cryproProvider = new RNGCryptoServiceProvider())
        {
            cryproProvider.GetBytes(byteArray);

            return Convert.ToBase64String(byteArray);
        }
    }

    private async Task<UserLoginResponseDto> SaveTokenDetails(string ipAddress, User user, string tokenString, string refreshToken)
    {
        var userRefreshToken = new UserRefreshToken
        {
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddDays(RefreshTokenTimeoutDays),
            IpAddress = ipAddress,
            IsInvalidated = false,
            RefreshToken = refreshToken,
            Token = tokenString,
            User = user
        };
        await _context.UserRefreshTokens.AddAsync(userRefreshToken);
        await _context.SaveChangesAsync();
        return new UserLoginResponseDto() {Token = tokenString, RefreshToken = refreshToken, IsSuccess = true};
    }
}