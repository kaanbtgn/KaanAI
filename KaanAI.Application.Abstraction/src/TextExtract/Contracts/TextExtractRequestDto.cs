using Microsoft.AspNetCore.Http;

namespace KaanAI.Application.Abstraction.TextExtract.Contracts;

public class TextExtractRequestDto
{
    public IFormFile File { get; set; } = default!;
    public string Language { get; set; } = "tr";
    public string? CourseName  { get; set; }
}
