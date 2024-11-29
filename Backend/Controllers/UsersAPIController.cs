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

[Route("api/users")]
[ApiController]
public class UsersAPIController : ControllerBase
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
            Email = model.Email
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.Like(u.Email, model.Email));

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

                return Ok(new
                {
                    token = tokenString
                });
            }
        }

        return Unauthorized();
    }



    [HttpGet("me")]
    public IActionResult Get()
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        // Parse the UserId
        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid User ID.");
        }

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
                ProfileVerified = new List<string>(){},
                LessonsCompleted = 0,
                Evaluations = 0,
                SkypeId = u.SkypeId,
                HangoutId = u.HangoutId,
                ProfileImage = u.ProfileImage
            })
            .FirstOrDefault();

        if (user == null)
        {
            return NotFound("User not found.");
        }

        return Ok(user);
    }
}
