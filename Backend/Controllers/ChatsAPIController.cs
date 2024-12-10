using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/chats")]
[ApiController]
public class ChatsAPIController : BaseController
{
    private readonly skillseekDbContext _dbContext;
    private readonly ILogger<ChatsAPIController> _logger;

    public ChatsAPIController(skillseekDbContext dbContext, ILogger<ChatsAPIController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetChats()
    {
        var userId = GetUserId();
        var chats = _dbContext.Chats
            .Where(c => c.StudentId == userId || c.TutorId == userId)
            .Include(c => c.Listing.LessonCategory)
            .Include(c => c.Messages)
            .ThenInclude(m => m.Sender)
            .Select(c => new ChatDto
            {
                Id = c.Id,
                ListingId = c.ListingId,
                TutorId = c.TutorId,
                StudentId = c.StudentId,
                RecipientId = c.StudentId == userId ? c.TutorId : c.StudentId,
                Details = c.StudentId == userId
                    ? $"{c.Listing.LessonCategory.Name} Tutor"
                    : $"{c.Listing.LessonCategory.Name} Student",
                Name = c.StudentId == userId ? c.Tutor.FirstName ?? c.Tutor.Email : c.Student.FirstName ?? c.Student.Email,
                LastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().Content ?? "No messages yet",
                Timestamp = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault().SentAt.ToString("g") ?? string.Empty,
                Messages = c.Messages.Select(m => new MessageDto
                {
                    Text = m.Content,
                    SentBy = m.SenderId == userId ? "me" : "contact",
                    Timestamp = m.SentAt.ToString("g")
                }).ToList(),
                RequestDetails = c.StudentId == userId ? "Chat Request by Student" : "Chat Request by Tutor"
            })
            .ToList();


        if (!chats.Any())
        {
            return NotFound("No chats found for this user.");
        }

        return Ok(chats);
    }

    [HttpPost("send")]
    public IActionResult SendMessage([FromBody] SendMessageDto messageDto)
    {
        var senderId = GetUserId();

        var users = _dbContext.Users.ToList();

        if (messageDto.RecipientId == null || messageDto.RecipientId == 0)
        {
            var listing = _dbContext.Listings.FirstOrDefault(l => l.Id == messageDto.ListingId);
            if (listing == null)
            {
                return BadRequest("Invalid listing ID.");
            }
            messageDto.RecipientId = listing.UserId;
        }

        // Validate the messageDto
        if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content) || messageDto.RecipientId <= 0)
        {
            return BadRequest("Invalid message data.");
        }

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
                ListingId = messageDto.ListingId
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
}
