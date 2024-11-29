public class ChatDto
{
    public int Id { get; set; }
    public string Name { get; set; } // The name of the other participant
    public string LastMessage { get; set; }
    public string Timestamp { get; set; }
    public string Details { get; set; }
    public List<MessageDto> Messages { get; set; }
    public string RequestDetails { get; set; }
    public List<LessonDto> Lessons { get; set; }
}

public class MessageDto
{
    public string Text { get; set; }
    public string SentBy { get; set; } // "me" or "contact"
    public string Timestamp { get; set; }
}

public class SentMessageDto
{
    public int RecipientId { get; set; } // The ID of the message recipient
    public string Content { get; set; } // The message text
}

public class LessonDto
{
    public DateTime Date { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal Price { get; set; }
    public int TutorId { get; set; }
}
