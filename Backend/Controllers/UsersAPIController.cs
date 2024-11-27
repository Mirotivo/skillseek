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

    private readonly skillseekDbContext dbContext;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IConfiguration _config;

    public UsersAPIController(ILogger<UsersAPIController> logger,
        Dictionary<string, string> users, IHostEnvironment hostingEnvironment, skillseekDbContext dbContext, IPasswordHasher<User> passwordHasher, IConfiguration config)
    {
        _logger = logger;
        _users = users;
        _hostingEnvironment = hostingEnvironment;
        this.dbContext = dbContext;
        this.passwordHasher = passwordHasher;
        _config = config;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Check if user already exists
        if (await dbContext.Users.AnyAsync(u => u.Email == model.Email))
        {
            return BadRequest("A user with this email address already exists.");
        }

        // Create new user
        User user = new User
        {
            Email = model.Email
        };
        user.PasswordHash = passwordHasher.HashPassword(user, model.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        return Ok();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user != null)
        {
            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

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


}
