namespace KaanAI.Domain;

public class Question
{
    public int Id { get; set; }

    public string Content { get; set; } = string.Empty; // girilen promptu burada tutacağız

    public DateTime AskedAt { get; set; } = DateTime.UtcNow;

    public virtual ChatSession Session { get; set; } = null!; //navigation property

    public int SessionId { get; set; }
}