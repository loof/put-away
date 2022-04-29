using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PutAway.Server.Data;
using PutAway.Shared.Entities;

using System.IdentityModel.Tokens.Jwt;

using System.Security.Cryptography;


using Microsoft.IdentityModel.Tokens;
using PutAway.Shared.Entities.Dto;


namespace PutAway.Server.Controllers;

[AllowAnonymous]
[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private const string DefaultUserRoleName = "User";

    public UsersController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }
    
    [HttpGet("GoogleSignIn")]
    public async Task GoogleSignIn()
    {
        await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties {RedirectUri = "/api/items"});
    }

    [HttpGet("getcurrentuser")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        User currentUser = new User();
        
        if (User.Identity.IsAuthenticated)
        {
            currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
            currentUser = await _context.Users.Where(u => u.EmailAddress == currentUser.EmailAddress).FirstOrDefaultAsync();

            if (currentUser == null)
            {
                currentUser = new User();
                currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
                _context.Users.Add(currentUser);
                await _context.SaveChangesAsync();
            }
        }

        return await Task.FromResult(currentUser);
    }
    
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<User>> Register(UserRegisterDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == request.EmailAddress);
        if (user != null)
        {
            return BadRequest("User already exists");
        }

        user = new User();
        
        CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        user.EmailAddress = request.EmailAddress;
        user.PasswordHash = passwordHash;
        user.PasswordSalt = passwordSalt;

        Role role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == DefaultUserRoleName);
        if (role != null)
        {
            user.Roles.Add(role);
        }
        else
        {
            user.Roles.Add(new Role {Name = DefaultUserRoleName});
        }
        
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new UserDto{Id = user.Id, EmailAddress = user.EmailAddress});
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Login(UserLoginDto request)
    {
        var user = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.EmailAddress == request.EmailAddress);
        if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return BadRequest("Wrong email or password");
        }

        string token = CreateToken(user);
        
        return Ok(token);
    }

    #region Private Methods

    private string CreateToken(User user)
    {
        List<Claim> claims = new List<Claim>
        {
            new (ClaimTypes.Email, user.EmailAddress)
        };

        foreach (Role userRole in user.Roles)
        {
            claims.Add(new (ClaimTypes.Role, userRole.Name));
        }

        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration.GetSection("Authentication:JWT:Token").Value));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(claims: claims, expires: DateTime.Now.AddDays(1), signingCredentials: creds);

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

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        }
    }

    #endregion
}