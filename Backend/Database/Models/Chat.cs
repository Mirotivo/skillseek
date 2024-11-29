using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Chat
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int StudentId { get; set; }

    [ForeignKey(nameof(Chat.StudentId))]
    public User Student { get; set; }

    [Required]
    public int TutorId { get; set; }

    [ForeignKey(nameof(Chat.TutorId))]
    public User Tutor { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation property for the associated messages
    public List<Message> Messages { get; set; }

    public Chat()
    {
        Messages = new List<Message>();
        CreatedAt = DateTime.UtcNow;
    }
}
