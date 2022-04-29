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
using PutAway.Server.Services;
using PutAway.Server.Services.Jwt;
using PutAway.Shared.Entities.Dto;

namespace PutAway.Server.Controllers;

[AllowAnonymous]
[Route("[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    #region Private Fields

    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private const int RefreshTokenTimeoutSeconds = 5;

    #endregion

    #region Constructor

    public UsersController(ApplicationDbContext context, IConfiguration configuration, IUserService userService, IJwtService jwtService)
    {
        _context = context;
        _configuration = configuration;
        _userService = userService;
        _jwtService = jwtService;
    }

    #endregion

    #region Public Methods

    [HttpGet("google-signin")]
    public async Task GoogleSignIn()
    {
        await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
            new AuthenticationProperties {RedirectUri = "/api/items"});
    }

    [HttpGet("get-current-user")]
    public async Task<ActionResult<User>> GetCurrentUser()
    {
        User currentUser = new User();

        if (User.Identity.IsAuthenticated)
        {
            currentUser.EmailAddress = User.FindFirstValue(ClaimTypes.Email);
            currentUser = await _context.Users.Where(u => u.EmailAddress == currentUser.EmailAddress)
                .FirstOrDefaultAsync();

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
    public async Task<ActionResult<User>> Register(UserRegisterRequestDto request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.EmailAddress == request.EmailAddress);
        if (user != null)
        {
            return BadRequest("User already exists.");
        }

        _userService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        user = new User
        {
            EmailAddress = request.EmailAddress,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        await _userService.AssignDefaultRole(user);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new UserRegisterResponseDto {EmailAddress = user.EmailAddress});
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Login(UserLoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new UserLoginResponseDto
            {
                IsSuccess = false,
                Reason = "Email and password must be provided."
            });
        }

        var user = _context.Users.Include(u => u.Roles).FirstOrDefault(u => u.EmailAddress == request.EmailAddress);
        if (user == null || !_userService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            return BadRequest("Wrong email or password.");
        }

        string token = _jwtService.CreateToken(user);
        string refreshTokenString = _jwtService.GenerateRefreshTokenString();
        string ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
        var userRefreshToken = new UserRefreshToken
        {
            CreatedDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddMinutes(RefreshTokenTimeoutSeconds),
            IpAddress = ipAddress,
            IsInvalidated = false,
            RefreshToken = refreshTokenString,
            Token = token,
            User = user
        };

        await _context.UserRefreshTokens.AddAsync(userRefreshToken);
        await _context.SaveChangesAsync();

        return Ok(new UserLoginResponseDto
        {
            Token = token,
            RefreshToken = refreshTokenString,
            IsSuccess = true
        });
    }

    [HttpGet("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new UserLoginResponseDto
            {
                IsSuccess = false,
                Reason = "Tokens must be provided."
            });
        }

        var token = _jwtService.GetJwtToken(request.ExpiredToken);
        var userRefreshToken = _context.UserRefreshTokens.FirstOrDefault(
            urt =>
                urt.IsInvalidated == false && urt.Token == request.ExpiredToken &&
                urt.RefreshToken == request.RefreshToken &&
                urt.IpAddress == HttpContext.Connection.RemoteIpAddress.ToString());

        UserLoginResponseDto response = ValidateDetails(token, userRefreshToken);
        if (!response.IsSuccess)
        {
            return BadRequest(response);
        }

        return Ok();
    }

    private UserLoginResponseDto ValidateDetails(JwtSecurityToken token, UserRefreshToken userRefreshToken)
    {
        if (userRefreshToken == null)
        {
            return new UserLoginResponseDto {IsSuccess = false, Reason = "Invalid token details."};
        }

        if (token.ValidTo > DateTime.UtcNow)
        {
            return new UserLoginResponseDto
            {
                IsSuccess = false,
                Reason = "Token not expired."
            };
        }

        if (!userRefreshToken.IsActive)
        {
            return new UserLoginResponseDto
            {
                IsSuccess = false,
                Reason = "Refresh token expired."
            };
        }

        return new UserLoginResponseDto
        {
            IsSuccess = true
        };
    }
    
    #endregion
    
}