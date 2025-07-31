namespace KaanAI.Domain;
public class Answer
{
    public int Id { get; set; }

    public string AnswerText { get; set; } = string.Empty; //dönen answer tutulacak.

    public DateTime AnsweredAt { get; set; } = DateTime.UtcNow;

    public virtual ChatSession Session { get; set; } = null!;

    public int SessionId { get; set; }
}