using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Infrastructure;
using Infrastructure.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<ActionResult<Object>> LogIn([FromBody] UserLoginModel userLoginModel) {

        var user = await _DbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == userLoginModel.Username && u.Password == userLoginModel.Password);

        if (user is null) {
            return Unauthorized("Invalid user and password combination");
        }

        if (!user.IsEnabled) {
            return Unauthorized("Your user has not been activated");
        }

        var token = GenerateJwtToken(user);
        return Ok(new { token = token });
    }

    [HttpPost("register")]
    [ActionName("Register")]
    public async Task<IActionResult> Register([FromBody] UserLoginModel user) {

        // Set the first user to register to be an admin and enabled
        var firstUser = await _DbContext.Users.AnyAsync();

        if (await _DbContext.Users.AnyAsync(u => u.Username == user.Username)) {
            return Conflict("Username already exists");
        }

        _DbContext.Users.Add(new User {
            Id = Guid.NewGuid(),
            Username = user.Username,
            Password = user.Password,
            IsAdmin = !firstUser,
            IsEnabled = !firstUser,
        });

        await _DbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost("change-password")]
    [Authorize]
    [ActionName("ChangePassword")]
    public async Task<IActionResult> ChangePassword(ChangePasswordModel request) {

        var username = User.Claims.First(c => c.Type == ClaimTypes.Name).Value;

        var user = await _DbContext.Users.FirstOrDefaultAsync(u => u.Username == username && u.Password == request.OldPassword);
        if (user is null) {
            return Unauthorized();
        }

        user.Password = request.NewPassword;
        await _DbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpGet("list-users")]
    [Authorize(Roles = "Admin")]
    [ActionName("ListUsers")]
    public async Task<IActionResult> ListUsers() {
        var users = await _DbContext.Users
            .AsNoTracking()
            .Select(u => new {
                UserId = u.Id,
                Username = u.Username,
                IsEnabled = u.IsEnabled,
                IsAdmin = u.IsAdmin,
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("modify-user")]
    [Authorize(Roles = "Admin")]
    [ActionName("ModifyUser")]
    public async Task<IActionResult> ModifyUser(EditUserModel editUserModel) {

        var user = await _DbContext.Users.FirstOrDefaultAsync(u => u.Id == editUserModel.userId);
        if (user is null) {
            return NotFound("User not found");
        }

        user.IsEnabled = editUserModel.isEnabled;
        user.IsAdmin = editUserModel.isAdmin;

        await _DbContext.SaveChangesAsync();

        return Ok();
    }

    private string GenerateJwtToken(User user) {
        var claims = new[] {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.IsAdmin ? "Admin" : "User")
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


    public record UserLoginModel(string Username, string Password);
    public record ChangePasswordModel(string OldPassword, string NewPassword);
    public record EditUserModel(Guid userId, bool isEnabled, bool isAdmin);
}
