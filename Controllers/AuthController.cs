using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProductApi.Models;

namespace ProductApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    public AuthController(IConfiguration configuration) => _configuration = configuration;

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
    {
        if (request.Username != "student" || request.Password != "12345")
            return Unauthorized(new { message = "Invalid username or password." });

        var jwt = _configuration.GetSection("Jwt");
        var expiresAt = DateTime.UtcNow.AddMinutes(
            int.TryParse(jwt["ExpiresMinutes"], out var minutes) ? minutes : 30);
        var claims = new[] {
            new Claim(ClaimTypes.Name, request.Username),
            new Claim(ClaimTypes.Role, "Student"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SigningKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"], claims,
            notBefore: DateTime.UtcNow, expires: expiresAt, signingCredentials: credentials);
        return Ok(new LoginResponse {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer", ExpiresAtUtc = expiresAt
        });
    }
}
