using System.ComponentModel;
using Microsoft.SemanticKernel;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.TextExtract;

namespace KaanAI.Application.Plugins;

public class SummaryPlugin
{
    private readonly ITextExtract _textExtract;
    private readonly Kernel _kernel;

    public SummaryPlugin(ITextExtract textExtract, Kernel kernel)
    {
        _textExtract = textExtract;
        _kernel = kernel;
    }

    [KernelFunction("summarize_text")]
    [Description("Summarize the provided text in Turkish with key points and a short abstract.")]
    public async Task<string> SummarizeTextAsync(
        [Description("The text content to summarize")] string content,
        [Description("Optional: summary length hint. e.g., 'short', 'medium', 'detailed'")] string length = "medium",
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"Aşağıdaki metni Türkçe olarak özetle.
        - Önce kısa bir özet ver.
        - Ardından madde işaretleriyle en önemli 5-10 noktayı çıkar.
        - Varsa terimler ve tanımlar için kısa bir sözlük bölümü ekle.
        - Uzunluk: {length}.

        Metin:
        '''
        {content}
        '''";

        var fn = _kernel.CreateFunctionFromPrompt(prompt);
        var result = await _kernel.InvokeAsync(fn, new KernelArguments(), cancellationToken);
        return result.GetValue<string>() ?? string.Empty;
    }

    [KernelFunction("extract_headings")]
    [Description("Extract important headings and subheadings from the provided text.")]
    public async Task<string> ExtractHeadingsAsync(
        [Description("The text content to analyze")] string content,
        [Description("Return format: 'bulleted' or 'outline'")] string format = "outline",
        CancellationToken cancellationToken = default)
    {
        var prompt = $@"Metinden önemli başlık ve alt başlıkları çıkar. Çıktıyı {format} formatında ver.
        - Düzgün numaralandırma veya madde işaretleri kullan.
        - Başlıkları öz ve net yaz.

        Metin:
        '''
        {content}
        '''";

        var fn = _kernel.CreateFunctionFromPrompt(prompt);
        var result = await _kernel.InvokeAsync(fn, new KernelArguments(), cancellationToken);
        return result.GetValue<string>() ?? string.Empty;
    }

    // Note: Kept non-exposed (no KernelFunction attribute) to avoid the LLM calling this with non-existent paths
    public async Task<string> SummarizePdfAsync(
        [Description("Absolute path to the PDF file")] string pdfPath,
        [Description("Optional: summary length hint")] string length = "medium",
        CancellationToken cancellationToken = default)
    {
        var raw = _textExtract.Extract(pdfPath);
        var normalized = _textExtract.Normalize(raw);
        return await SummarizeTextAsync(normalized, length, cancellationToken);
    }
}