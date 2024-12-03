public class ChatDto
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string Name { get; set; }
    public string LastMessage { get; set; }
    public string Timestamp { get; set; }
    public string Details { get; set; }
    public List<MessageDto> Messages { get; set; }
    public string RequestDetails { get; set; }
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