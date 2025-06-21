using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MusicService.DAL.PostgreSQL;
using MusicService.Domain.Entities;
using MusicService.WebApi.Contracts.Requests;
using MusicService.WebApi.Contracts.Responses;

namespace MusicService.WebApi.Controllers;

public class AuthController(AuthOptions authOptions, MusicServiceDbContext dbContext) : BaseController(authOptions)
{
    private readonly AuthOptions _authOptions = authOptions;

    [HttpPost("sign-in")]
    public async Task<IActionResult> SignIn(
        SignInRequest request, 
        bool useCookies = false,
        bool rememberMe = true)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(e => e.Email == request.EmailOrUsername) 
            ?? await dbContext.Users.FirstOrDefaultAsync(e => e.Username == request.EmailOrUsername);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return BadRequest("Некорректный логин или пароль.");
        }

        if (!useCookies)
        {
            return Ok(new TokensResponse(GenerateJwtAccessToken(user)));
        }
        
        var principal = CreateUserPrincipal(user, CookieAuthenticationDefaults.AuthenticationScheme, _authOptions);
        await HttpContext.SignInAsync(principal, new AuthenticationProperties { IsPersistent = rememberMe });
            
        return Ok();
    }

    [HttpPost("sign-up")]
    public async Task<IActionResult> SignUp(SignUpRequest request)
    {
        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = Role.User,
            Id = Guid.NewGuid(),
        };

        if (await dbContext.Users.AnyAsync(e => e.Email == request.Email))
        {
            return BadRequest("Аккаунт с таким email уже существует.");
        }

        if (await dbContext.Users.AnyAsync(e => e.Username == request.Username))
        {
            return BadRequest("Аккаунт с таким username уже существует.");
        }
        
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        
        return Ok(user.Id);
    }
    
    private string GenerateJwtAccessToken(User user)
    {
        using var scope = HttpContext.RequestServices.CreateScope();
        var timeProvider = scope.ServiceProvider.GetRequiredService<TimeProvider>();

        var currentDateTime = timeProvider.GetUtcNow().UtcDateTime;
        var expires = currentDateTime.Add(_authOptions.AccessTokenLifetime);
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.SecretKey)),
            SecurityAlgorithms.HmacSha256
        );
        
        var principal = CreateUserPrincipal(user, JwtBearerDefaults.AuthenticationScheme, _authOptions);
        
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _authOptions.Issuer,
            Audience = _authOptions.Audience,
            Claims = principal.Claims.ToDictionary(c => c.Type, c => (object)c.Value),
            Expires = expires,
            SigningCredentials = signingCredentials,
            NotBefore = currentDateTime,
            IssuedAt = currentDateTime,
        };
        
        var tokenValue = new JsonWebTokenHandler().CreateToken(descriptor);
        return tokenValue;
    }

    [Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
    [HttpPost("sign-out")]
    public async Task<IActionResult> SignOutEndpoint()
    {
        await HttpContext.SignOutAsync();
        return Ok();
    }

    [Authorize(AuthenticationSchemes = $"{CookieAuthenticationDefaults.AuthenticationScheme},{JwtBearerDefaults.AuthenticationScheme}")]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (GetCurrentUserId() is not { } userId)
        {
            return Unauthorized();
        }

        var user = await dbContext
            .Users
            .FirstOrDefaultAsync(e => e.Id == userId);

        if (user is null)
        {
            return Unauthorized();
        }
        
        return Ok(new
        {
            user.Id, 
            user.Email, 
            user.Username, 
            user.Role 
        });
    }
    
    private static ClaimsPrincipal CreateUserPrincipal(User user, string authenticationType, AuthOptions authOptions)
    {
        var result = new ClaimsIdentity(
            authenticationType, 
            authOptions.UsernameClaimName, 
            authOptions.RoleClaimName);
        
        result.AddClaim(new Claim(authOptions.UsernameClaimName, user.Username));
        result.AddClaim(new Claim(authOptions.RoleClaimName, user.Role.ToString()));
        result.AddClaim(new Claim(authOptions.UserIdClaimName, user.Id.ToString()));
        result.AddClaim(new Claim(authOptions.EmailClaimName, user.Email));
        result.AddClaim(new Claim("auth_type", authenticationType));
        
        return new ClaimsPrincipal(result);
    }
}