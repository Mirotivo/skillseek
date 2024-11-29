using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/chats")]
[ApiController]
public class ChatsAPIController : ControllerBase
{
    private readonly skillseekDbContext _dbContext;

    public ChatsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet()]
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


        // Query chats where the user is either a student or a tutor
        var chats = _dbContext.Chats
            .Where(c => c.StudentId == userId || c.TutorId == userId)
            .Include(c => c.Messages) // Include related messages
            .ThenInclude(m => m.Sender) // Include sender details for each message
            .Select(c => new ChatDto
            {
                Id = c.Id,
                Name = c.StudentId == userId ? c.Tutor.FirstName : c.Student.FirstName,
                LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().Content ?? "No messages yet",
                Timestamp = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().SentAt.ToString("g") ?? string.Empty,
                Details = c.StudentId == userId ? $"{c.Tutor.Email}, Tutor" : $"{c.Student.Email}, Student",
                Messages = c.Messages.Select(m => new MessageDto
                {
                    Text = m.Content,
                    SentBy = m.SenderId == userId ? "me" : "contact",
                    Timestamp = m.SentAt.ToString("g") // Format the timestamp
                }).ToList(),
                RequestDetails = c.StudentId == userId ? "Chat Request by Student" : "Chat Request by Tutor",
                Lessons = new List<LessonDto>() // Placeholder for lessons, if applicable
            })
            .ToList();


        if (!chats.Any())
        {
            return NotFound("No chats found for this user.");
        }

        return Ok(chats);
    }

    [HttpPost("send")]
    public IActionResult Send([FromBody] SentMessageDto messageDto)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        // Parse the UserId
        if (!int.TryParse(userIdClaim.Value, out var senderId))
        {
            return BadRequest("Invalid User ID.");
        }

        // Validate the messageDto
        if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content) || messageDto.RecipientId <= 0)
        {
            return BadRequest("Invalid message data.");
        }

        try
        {
            // Check if a chat already exists
            var chat = _dbContext.Chats.FirstOrDefault(c =>
                (c.StudentId == senderId && c.TutorId == messageDto.RecipientId) ||
                (c.StudentId == messageDto.RecipientId && c.TutorId == senderId));

            if (chat == null)
            {
                // Create a new chat if it doesn't exist
                chat = new Chat
                {
                    StudentId = senderId, // Assuming the sender is a student
                    TutorId = messageDto.RecipientId,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Chats.Add(chat);
                _dbContext.SaveChanges(); // Save to generate ChatId
            }

            // Add the message to the chat
            var message = new Message
            {
                ChatId = chat.Id,
                SenderId = senderId,
                RecipientId = messageDto.RecipientId,
                Content = messageDto.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _dbContext.Messages.Add(message);
            _dbContext.SaveChanges();

            return Ok(new { success = true, message = "Message sent successfully." });
        }
        catch (Exception ex)
        {
            // Log the exception (not shown here for brevity)
            return StatusCode(500, "An error occurred while sending the message.");
        }
    }
}
