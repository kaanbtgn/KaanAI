namespace KaanAI.Application.Abstraction.Chat.Contracts;

public class AddAnswerRequest
{
    public int SessionId { get; set; }
    public string AnswerText { get; set; } = string.Empty;
} 