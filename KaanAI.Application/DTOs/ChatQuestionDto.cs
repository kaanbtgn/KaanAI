using Microsoft.AspNetCore.Http;

namespace KaanAI.Application.DTOs;

public class ChatQuestionDto
{
    public string Prompt { get; set; } = string.Empty;
    public IFormFile? File { get; set; }
}