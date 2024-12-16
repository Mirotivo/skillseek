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
    private readonly IChatService _chatService;
    private readonly IListingService _listingService;
    private readonly ILogger<ChatsAPIController> _logger;

    public ChatsAPIController(
        IChatService chatService,
        IListingService listingService,
        ILogger<ChatsAPIController> logger
    )
    {
        _chatService = chatService;
        _listingService = listingService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetChats()
    {
        var userId = GetUserId();
        var chats = _chatService.GetChats(userId);

        if (!chats.Any())
        {
            return NotFound("No chats found for this user.");
        }

        return Ok(chats);
    }

    [HttpPost("send")]
    public IActionResult SendMessage([FromBody] SendMessageDto messageDto)
    {
        // Validate the messageDto
        if (messageDto.RecipientId == null || messageDto.RecipientId == 0)
        {
            var listing = _listingService.GetListingById(messageDto.ListingId);
            if (listing == null)
            {
                return BadRequest("Invalid listing ID.");
            }
            messageDto.RecipientId = listing.TutorId;
        }
        if (messageDto == null || string.IsNullOrWhiteSpace(messageDto.Content) || messageDto.RecipientId <= 0)
        {
            return BadRequest("Invalid message data.");
        }

        // Check if a chat already exists
        var senderId = GetUserId();

        if (!_chatService.SendMessage(messageDto, senderId))
        {
            return BadRequest("Failed to send the message.");
        }

        return Ok(new { success = true, message = "Message sent successfully." });
    }
}
