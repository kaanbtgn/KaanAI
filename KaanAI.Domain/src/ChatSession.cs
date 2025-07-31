namespace KaanAI.Domain;

public class ChatSession
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}