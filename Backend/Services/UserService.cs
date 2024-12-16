using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

public class UserService : IUserService
{
    private readonly skillseekDbContext _dbContext;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IConfiguration _config;

    public UserService(skillseekDbContext dbContext, IPasswordHasher<User> passwordHasher, IConfiguration config)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _config = config;
    }

    public async Task<bool> RegisterUser(RegisterViewModel model)
    {
        if (await _dbContext.Users.AnyAsync(u => EF.Functions.Like(u.Email, model.Email)))
        {
            return false; // User already exists
        }

        var user = new User
        {
            Email = model.Email,
            Roles = Role.Tutor
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, model.Password);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        if (!string.IsNullOrEmpty(model.ReferralToken))
        {
            var referrer = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == model.ReferralToken);
            if (referrer != null)
            {
                _dbContext.Referrals.Add(new Referral
                {
                    ReferrerId = referrer.Id,
                    ReferredId = user.Id
                });

                _dbContext.Subscriptions.Add(new Subscription
                {
                    UserId = referrer.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(1),
                    Amount = 0,
                    Status = SubscriptionStatus.Active,
                    Plan = "Premium"
                });
            }
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<(string Token, List<string> Roles)?> LoginUser(LoginViewModel model)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.Like(u.Email, model.Email));
        if (user == null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) != PasswordVerificationResult.Success)
        {
            return null; // Invalid credentials
        }

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

        var roles = Enum.GetValues(typeof(Role))
            .Cast<Role>()
            .Where(r => r != Role.None && user.Roles.HasFlag(r))
            .Select(r => r.ToString())
            .ToList();

        return (tokenString, roles);
    }

    public async Task<UserDto?> GetUser(int userId)
    {
        return await _dbContext.Users
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
                ProfileVerified = new List<string>(),
                RecommendationToken = u.RecommendationToken,
                LessonsCompleted = _dbContext.Lessons.Include(l => l.Listing).Count(l => l.Listing.UserId == userId && l.Status == LessonStatus.Completed),
                Evaluations = _dbContext.Reviews.Count(r => r.RevieweeId == userId && (r.Type == ReviewType.Review || r.Type == ReviewType.Recommendation)),
                PaymentDetailsAvailable = !string.IsNullOrEmpty(u.StripeCustomerId)
            })
            .FirstOrDefaultAsync();
    }

    public async Task<UserDto?> GetUserByToken(string recommendationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.RecommendationToken == recommendationToken);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            Phone = user.Phone,
            Address = user.Address,
            ProfileImage = user.ProfileImage,
            SkypeId = user.SkypeId,
            HangoutId = user.HangoutId,
            ProfileVerified = new List<string>()
        };
    }

    public async Task<bool> UpdateUser(int userId, UserDto updatedUser)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.FirstName = updatedUser.FirstName ?? user.FirstName;
        user.LastName = updatedUser.LastName ?? user.LastName;
        user.DateOfBirth = updatedUser.DateOfBirth ?? user.DateOfBirth;
        user.Phone = updatedUser.Phone ?? user.Phone;
        user.Address = updatedUser.Address ?? user.Address;
        user.ProfileImage = updatedUser.ProfileImage ?? user.ProfileImage;
        user.SkypeId = updatedUser.SkypeId ?? user.SkypeId;
        user.HangoutId = updatedUser.HangoutId ?? user.HangoutId;

        await _dbContext.SaveChangesAsync();
        return true;
    }
}
