namespace KaanAI.Application.DTOs;

public class MessageDto
{
    public int SessionId { get; init; }
    public string Question { get; init; } = string.Empty;
    public string Answer { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}