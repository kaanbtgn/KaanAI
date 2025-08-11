using System.Text;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.TextExtract;

namespace KaanAI.Application;

public class TextExtractService : ITextExtract
{
    public string Extract(string pdfPath)
    {
        if (string.IsNullOrWhiteSpace(pdfPath))
        {
            throw new ArgumentException("PDF path cannot be null or empty.", nameof(pdfPath));
        }

        if (!File.Exists(pdfPath))
        {
            throw new FileNotFoundException("PDF file not found", pdfPath);
        }

        var builder = new StringBuilder();

        using var document = PdfDocument.Open(pdfPath);
        foreach (var page in document.GetPages())
        {
            AppendPageText(page, builder);
            builder.AppendLine();
            builder.AppendLine("\f"); // page break marker
        }

        return builder.ToString();
    }

    public string Normalize(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var normalized = text;

        // Remove common hyphenation at line breaks: "exam-\nple" -> "example"
        normalized = normalized.Replace("-\n", string.Empty)
                               .Replace("-\r\n", string.Empty);

        // Normalize line endings
        normalized = normalized.Replace("\r\n", "\n");

        // Collapse multiple blank lines
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, "\n{3,}", "\n\n");

        // Trim trailing spaces on each line
        var lines = normalized.Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].TrimEnd();
        }

        normalized = string.Join('\n', lines);

        return normalized.Trim();
    }

    private static void AppendPageText(Page page, StringBuilder builder)
    {
        var words = page.GetWords();
        if (!words.Any()) return;

        // Group words into lines using the Y-center of the bounding box (rounded)
        var lineGroups = words
            .GroupBy(w => Math.Round((w.BoundingBox.Bottom + w.BoundingBox.Top) / 2.0, 0))
            .OrderByDescending(g => g.Key); // PDF coordinate Y grows upwards

        foreach (var line in lineGroups)
        {
            var lineText = string.Join(" ", line
                .OrderBy(w => w.BoundingBox.Left)
                .Select(w => w.Text));

            builder.AppendLine(lineText);
        }
    }
}