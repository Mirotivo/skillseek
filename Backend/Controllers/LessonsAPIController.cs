using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/lessons")]
[ApiController]
public class LessonsAPIController : ControllerBase
{
    private readonly skillseekDbContext _dbContext;

    public LessonsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("proposeLesson")]
    public IActionResult ProposeLesson([FromBody] LessonDto lessonDto)
    {
        var userIdClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return Unauthorized("User ID not found in token.");
        }

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return BadRequest("Invalid User ID.");
        }

        var lesson = new Lesson
        {
            Date = lessonDto.Date,
            Duration = lessonDto.Duration,
            Price = lessonDto.Price,
            StudentId = userId,
            TutorId = lessonDto.TutorId,
            IsStudentInitiator = true,
            Status = LessonStatus.Proposed
        };

        _dbContext.Lessons.Add(lesson);
        _dbContext.SaveChanges();

        return Ok(new { Message = "Lesson proposed successfully.", LessonId = lesson.Id });
    }

    [HttpGet("{contactId}")]
    public IActionResult GetPropositions(int contactId)
    {
        var propositions = _dbContext.Lessons
            .Where(l => l.TutorId == contactId || l.StudentId == contactId)
            .Select(l => new
            {
                l.Date,
                l.Duration,
                l.Price,
                l.Status
            })
            .ToList();

        if (!propositions.Any())
        {
            return NotFound("No propositions found for this contact.");
        }

        return Ok(propositions);
    }
}

