using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;
using skillseek.Controllers;

[Route("api/users")]
[ApiController]
public class UsersAPIController : BaseController
{

    private readonly ILogger<UsersAPIController> _logger;
    private readonly Dictionary<string, string> _users;
    private readonly IHostEnvironment _hostingEnvironment;

    private readonly skillseekDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _config;

    public UsersAPIController(ILogger<UsersAPIController> logger,
        Dictionary<string, string> users, IHostEnvironment hostingEnvironment, skillseekDbContext dbContext, IPasswordHasher<User> passwordHasher, IConfiguration config)
    {
        _logger = logger;
        _users = users;
        _hostingEnvironment = hostingEnvironment;
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Check if user already exists
        if (await _dbContext.Users.AnyAsync(u => EF.Functions.Like(u.Email, model.Email)))
        {
            return BadRequest("A user with this email address already exists.");
        }

        // Create new user
        User user = new User
        {
            Email = model.Email,
            Roles = Role.Tutor
            // Roles = Role.Student
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => EF.Functions.Like(u.Email, model.Email));

        if (user != null)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Success)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddDays(7),
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "")),
                        SecurityAlgorithms.HmacSha256Signature)
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                Console.WriteLine("Generated Token: " + tokenString);

                // Decode the roles into a list of role names
                var roles = Enum.GetValues(typeof(Role))
                    .Cast<Role>()
                    .Where(r => r != Role.None && user.Roles.HasFlag(r))
                    .Select(r => r.ToString())
                    .ToList();

                return Ok(new
                {
                    token = tokenString,
                    roles = roles
                });
            }
        }

        return Unauthorized();
    }



    [HttpGet("me")]
    public IActionResult Get()
    {
        var userId = GetUserId();

        // Fetch the user from the database or repository
        var user = _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                DateOfBirth = u.DateOfBirth,
                Phone = u.Phone,
                Address = u.Address,
                ProfileImage = u.ProfileImage,
                SkypeId = u.SkypeId,
                HangoutId = u.HangoutId,
                ProfileVerified = new List<string>() { },
                LessonsCompleted = 0,
                Evaluations = 0,
            })
            .FirstOrDefault();

        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }

    [HttpPut("me")]
    public IActionResult Update([FromBody] UserDto updatedUser)
    {
        var userId = GetUserId();

        // Fetch the existing user from the database
        var user = _dbContext.Users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Update only changed fields
        user.FirstName = updatedUser.FirstName ?? user.FirstName;
        user.LastName = updatedUser.LastName ?? user.LastName;
        user.DateOfBirth = updatedUser.DateOfBirth ?? user.DateOfBirth;
        user.Phone = updatedUser.Phone ?? user.Phone;
        user.Address = updatedUser.Address ?? user.Address;
        user.ProfileImage = updatedUser.ProfileImage ?? user.ProfileImage;
        user.SkypeId = updatedUser.SkypeId ?? user.SkypeId;
        user.HangoutId = updatedUser.HangoutId ?? user.HangoutId;

        // Save changes to the database
        _dbContext.SaveChanges();

        return Ok(new { success = true, message = "Profile updated successfully." });
    }

}
