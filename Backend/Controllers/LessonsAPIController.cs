using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using skillseek.Models;

namespace skillseek.Controllers;

[Route("api/lessons")]
[ApiController]
public class LessonsAPIController : BaseController
{
    private readonly skillseekDbContext _dbContext;

    public LessonsAPIController(skillseekDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("proposeLesson")]
    public IActionResult ProposeLesson([FromBody] LessonDto lessonDto)
    {
        var userId = GetUserId();

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
        // Fetch Propositions
        var propositions = _dbContext.Lessons
            .Where(p => p.Status == LessonStatus.Proposed)
            .Where(p => p.StudentId == contactId)
            .Select(p => new PropositionDto
            {
                Id = p.Id,
                Date = p.Date,
                Duration = p.Duration.ToString(@"hh\:mm"),
                Price = p.Price,
                Status = p.Status.ToString()
            })
            .ToList();

        // Fetch Lessons
        var lessons = _dbContext.Lessons
            .Where(p => p.Status == LessonStatus.Booked || p.Status == LessonStatus.Completed || p.Status == LessonStatus.Canceled)
            .Where(l => l.StudentId == contactId)
            .Select(l => new LessonDto
            {
                Id = l.Id,
                Topic = "Lesson",
                Date = l.Date,
                Duration = l.Duration,
                Status = l.Status.ToString()
            })
            .ToList();

        return Ok(new
        {
            Propositions = propositions,
            Lessons = lessons
        });
    }
}

