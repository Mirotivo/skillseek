public class PropositionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; } // The proposed date for the lesson
    public string Duration { get; set; } // e.g., "1 hour"
    public decimal Price { get; set; } // e.g., 30.00
    public string Status { get; set; } // Status of the proposition (e.g., "Proposed")
}
