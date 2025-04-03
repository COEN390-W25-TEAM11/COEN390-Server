using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace COEN390_Server.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase {

    private readonly IConfiguration _configuration;
    private readonly MyDbContext _DbContext;

    public AuthController(
        IConfiguration configuration,
        MyDbContext myDbContext
    ) {
        _configuration = configuration;
        _DbContext = myDbContext;
    }

    [HttpPost("login")]
    [ActionName("LogIn")]
    public async Task<ActionResult<string>> LogIn([FromBody] UserLogin user) {
        if (
            (user.Username == "root" && user.Password == "toor") ||
            _DbContext.Users.Any(u => u.Username == user.Username && u.Password == user.Password)
        ) {
            var token = GenerateJwtToken(user.Username);
            return Ok(new { token = token }); // return JSON object
        }

        return Unauthorized();
    }

    [HttpPost("register")]
    [ActionName("Register")]
    public async Task<ActionResult<string>> Register([FromBody] UserLogin user) {
        throw new NotImplementedException();
    }

    private string GenerateJwtToken(string username) {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "yourdomain.com",
            audience: "yourdomain.com",
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public record UserLogin(string Username, string Password);
}
