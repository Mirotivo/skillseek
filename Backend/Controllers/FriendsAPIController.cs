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
using Microsoft.AspNetCore.Authorization;


[Route("api/friends")]
[ApiController]
public class FriendsAPIController : ControllerBase
{

    private readonly ILogger<FriendsAPIController> _logger;
    private readonly skillseekDbContext dbContext;

    public FriendsAPIController(ILogger<FriendsAPIController> logger,
        skillseekDbContext dbContext
    )
    {
        _logger = logger;
        this.dbContext = dbContext;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetFriends()
    {
        // Get the currently authenticated user's ID (you need to implement authentication)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized(); // User is not authenticated
        }

        try
        {
            // Convert the user ID claim value to an integer
            if (int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                // Fetch the list of friends for the current user
                var friends = await dbContext.Friendships
                    .Where(f => f.UserId == currentUserId)
                    .Include(f => f.Friend) // Include the Friend entity for each friendship
                    .Select(f => f.Friend)
                    .ToListAsync();

                return Ok(friends);
            }
            else
            {
                return BadRequest("Invalid user ID claim"); // Handle invalid user ID claim
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching friends.");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetFriendChat(int id)
    {
        var dummyChatMessages = new List<ChatMessage>
        {
            new ChatMessage { Id = 1, Text = "Hello!", Timestamp = DateTime.Now.AddMinutes(-10) },
            new ChatMessage { Id = 2, Text = "How are you?", Timestamp = DateTime.Now.AddMinutes(-5) },
            new ChatMessage { Id = 3, Text = "I'm good, thanks!", Timestamp = DateTime.Now },
        };
        return Ok(dummyChatMessages);
    }

}

public class ChatMessage
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime Timestamp { get; set; }

    public ChatMessage()
    {
    }
}
