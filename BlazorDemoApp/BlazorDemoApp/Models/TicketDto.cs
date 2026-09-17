namespace BlazorDemoApp.Models;

public class TicketDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? Resolution { get; set; }
}

public class ChatReplyDto
{
    public string Reply { get; set; } = string.Empty;
}

public class ChatMessageModel
{
    public string Text { get; set; } = string.Empty;
    public bool IsUser { get; set; }
    public DateTime Time { get; set; } = DateTime.Now;
}