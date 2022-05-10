using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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

    public async Task<User> AddAndSave(User user)
    {
        Task<User> addedUser = Add(user);
        await _context.SaveChangesAsync();
        return addedUser.Result;
    }

    public async Task<User> GetByFilterIncludeRoles(Expression<Func<User, bool>> filter)
    {
        User? user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(filter);
        return user;
    }

    public async Task<User> Add(User user)
    {
        EntityEntry<User> entityEntry = await _context.Users.AddAsync(user);
        return entityEntry.Entity;
    }

    public async Task<bool> UserExists(string emailAddress)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == emailAddress);
        return user != null;
    }
}