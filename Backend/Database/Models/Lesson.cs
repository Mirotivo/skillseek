using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public enum LessonStatus
{
    Proposed,
    Booked,
    Completed,
    Canceled
}

public class Lesson
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TimeSpan Duration { get; set; }

    [Required]
    public decimal Price { get; set; }

    public int StudentId { get; set; }

    [ForeignKey(nameof(Lesson.StudentId))]
    public User Student { get; set; }

    public int TutorId { get; set; }

    [ForeignKey(nameof(Lesson.TutorId))]
    public User Tutor { get; set; }

    [Required]
    public bool IsStudentInitiator { get; set; }

    [Required]
    [MaxLength(20)]
    public LessonStatus Status { get; set; }
}
