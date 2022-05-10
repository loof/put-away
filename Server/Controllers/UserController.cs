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

    #endregion

    #region Constructor

    public UsersController(ApplicationDbContext context, IConfiguration configuration, IUserService userService,
        IJwtService jwtService)
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
        if (await _userService.UserExists(request.EmailAddress))
        {
            return BadRequest("User already exists.");
        }

        _userService.CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

        User user = new User
        {
            EmailAddress = request.EmailAddress,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        await _userService.AssignDefaultRole(user);
        await _userService.AddAndSave(user);

        return Ok(new UserRegisterResponseDto {EmailAddress = user.EmailAddress});
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<string>> Login(UserLoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new UserLoginResponseDto
                {IsSuccess = false, Reason = "UserName and Password must be provided."});
        var authResponse = await _jwtService.GetTokenAsync(request, HttpContext.Connection.RemoteIpAddress.ToString());
        if (authResponse == null)
            return Unauthorized();
        return Ok(authResponse);
    }


    [HttpPost("[action]")]
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

        var ipAddress = HttpContext.Connection.RemoteIpAddress.ToString();
        var token = _jwtService.GetJwtToken(request.ExpiredToken);
        var userRefreshToken = _context.UserRefreshTokens.FirstOrDefault(
            urt =>
                urt.IsInvalidated == false && urt.Token == request.ExpiredToken &&
                urt.RefreshToken == request.RefreshToken &&
                urt.IpAddress == ipAddress);

        UserLoginResponseDto response = ValidateDetails(token, userRefreshToken);
        if (!response.IsSuccess)
        {
            return BadRequest(response);
        }

        userRefreshToken.IsInvalidated = true;
        _context.UserRefreshTokens.Update(userRefreshToken);
        await _context.SaveChangesAsync();

        var userEmail = token.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value;
        var user = await _context.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.EmailAddress == userEmail);
        
        var authResponse = await _jwtService.GetRefreshTokenAsync(ipAddress, user); 

        return Ok(authResponse);
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