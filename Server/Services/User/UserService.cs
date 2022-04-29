using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PutAway.Server.Data;
using PutAway.Shared.Entities;

namespace PutAway.Server.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private const string DefaultUserRoleName = "User";



    public UserService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512(passwordSalt))
        {
            var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            return computeHash.SequenceEqual(passwordHash);
        }
    }
    
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    public async Task AssignDefaultRole(User user)
    {
        Role role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == DefaultUserRoleName);
        if (role != null)
        {
            user.Roles.Add(role);
        }
        else
        {
            user.Roles.Add(new Role {Name = DefaultUserRoleName});
        }
    }


}