namespace KaanAI.Domain;

public class ErrorLog
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public int SessionId { get; set; }

    public string Message { get; set; }

    public string StackTrace { get; set; }

}