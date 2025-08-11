using KaanAI.Application.Abstraction.SemanticKernel;
using KaanAI.Application.Abstraction.SemanticKernel.Contracts;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.TextExtract;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers;

/// <summary>
/// Main AI Assistant API Controller
/// This controller provides a single unified endpoint that intelligently routes requests to appropriate plugins
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AssistantController : ControllerBase
{
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly ITextExtract _textExtract;
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(
        ISemanticKernelService semanticKernelService,
        ITextExtract textExtract,
        ILogger<AssistantController> logger)
    {
        _semanticKernelService = semanticKernelService;
        _textExtract = textExtract;
        _logger = logger;
    }
/// <summary>
/// JSON chat - send a plain prompt (no file upload) to the Assistant.
/// </summary>
[HttpPost("chat")]
[Consumes("application/json")]
[ProducesResponseType(typeof(SemanticKernelResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult> Chat([FromBody] SemanticKernelRequestDto request, CancellationToken ct)
{
    try
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Geçersiz istek gövdesi." });
        }
        _logger.LogInformation("Assistant request: {Message}", request.Message);
        var response = await _semanticKernelService.ExecuteAsync(request, ct);
        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Assistant error");
        return StatusCode(500, new { response = "Üzgünüm, isteğinizi işlerken bir hata oluştu.", isSuccess = false, errorMessage = ex.Message, createdAt = DateTime.UtcNow });
    }
}

/// <summary>
/// File upload chat - attach PDFs and send a prompt. The extracted text is appended to the message automatically.
/// </summary>
[HttpPost("chat-upload")]
[Consumes("multipart/form-data")]
[ProducesResponseType(typeof(SemanticKernelResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult> ChatUpload([FromForm] AssistantFormRequestDto request, CancellationToken ct)
{
    try
    {
        _logger.LogInformation("Assistant form request: {Message}, files: {Count}", request.Message, request.Files?.Count ?? 0);

        var attachmentsText = string.Empty;
        if (request.Files != null && request.Files.Count > 0)
        {
            var parts = new List<string>();
            foreach (var file in request.Files)
            {
                if (file is null || file.Length <= 0) continue;

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName));
                try
                {
                    await using (var fs = System.IO.File.Create(tempPath))
                    {
                        await file.CopyToAsync(fs, ct);
                    }

                    var extracted = _textExtract.Extract(tempPath);
                    var normalized = _textExtract.Normalize(extracted);

                    var snippet = normalized.Length > 20000 ? normalized[..20000] : normalized;
                    parts.Add($"===== Attachment: {file.FileName} =====\n{snippet}\n===== End Attachment =====");
                }
                finally
                {
                    try { System.IO.File.Delete(tempPath); } catch { /* ignore */ }
                }
            }

            if (parts.Count == 0)
            {
                return BadRequest(new { message = "Dosyalar okunamadı veya boş." });
            }
            attachmentsText = string.Join("\n\n", parts);
        }

        var combinedMessage = request.Message;
        if (!string.IsNullOrWhiteSpace(attachmentsText))
        {
            combinedMessage += "\n\nAşağıda yüklediğim dosya içerikleri bulunuyor. Lütfen isteğime göre bu içerikleri özetle/başlık çıkar veya uygun şekilde değerlendir:\n\n" + attachmentsText;
        }

        var skRequest = new SemanticKernelRequestDto
        {
            Message = combinedMessage,
            SessionId = request.SessionId,
            IncludeHistory = request.IncludeHistory,
            PreferredPlugin = request.PreferredPlugin,
            AutoDetectIntent = request.AutoDetectIntent
        };

        var response = await _semanticKernelService.ExecuteAsync(skRequest, ct);
        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Assistant form error");
        return StatusCode(500, new { response = "Üzgünüm, isteğinizi işlerken bir hata oluştu.", isSuccess = false, errorMessage = ex.Message, createdAt = DateTime.UtcNow });
    }
}

    /// <summary>
    /// Get information about available AI capabilities
    /// </summary>
    /// <returns>Information about supported plugins and their capabilities</returns>
    [HttpGet("capabilities")]
    [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult GetCapabilities()
    {
        var capabilities = new
        {
            plugins = new[]
            {
                new
                {
                    name = "CurrencyPlugin",
                    description = "Get currency/crypto/forex prices, OHLC candles and quick analysis",
                    examples = new[] 
                    { 
                        "BTC/EUR fiyatı nedir?", 
                        "EUR/USD kaç?",
                        "USD/TRY anlık fiyat",
                        "BTC/EUR için 1 saatlik OHLC verisi",
                        "DOGE/EUR price"
                    }
                },
                new
                {
                    name = "WeatherPlugin",
                    description = "Get current weather and forecasts for any location",
                    examples = new[] 
                    { 
                        "What's the weather in Istanbul?", 
                        "İstanbul'da hava durumu nasıl?",
                        "Weather forecast for Ankara",
                        "Bugün hava nasıl?"
                    }
                },
                new
                {
                    name = "GreetingPlugin",
                    description = "Handle greetings, introductions, and show available capabilities",
                    examples = new[] 
                    { 
                        "Merhaba",
                        "Hello",
                        "What can you do?",
                        "Neler yapabilirsin?"
                    }
                }
                    ,
                    new
                    {
                        name = "SummaryPlugin",
                        description = "Summarize text or PDF files, extract headings and key points. Works with file uploads via the same chat endpoint.",
                        examples = new[]
                        {
                            "Bu içeriği özetler misin?",
                            "Başlıkları ve alt başlıkları çıkar",
                            "PDF yükledim, önemli noktaları listele",
                            "Ders notlarındaki konuları başlık başlık çıkar"
                        }
                    }
            },
            features = new[]
            {
                "Intelligent plugin detection using LLM",
                "Multi-language support (Turkish & English)",
                "Session management",
                "Context-aware responses",
                "Real-time crypto data via Binance API (free)",
                "Weather data via OpenWeatherMap",
                "AI-powered analysis via Azure OpenAI",
                    "File uploads (multipart/form-data) on the same chat endpoint to attach PDFs"
            },
            supportedCryptos = new[]
            {
                "BTC", "ETH", "ADA", "DOGE", "LTC", "XRP", "BNB", "SOL", "MATIC", "AVAX"
            },
            endpoints = new
            {
                    chat = "/api/Assistant/chat" , // supports application/json and multipart/form-data
                capabilities = "/api/Assistant/capabilities",
                health = "/api/Assistant/health"
            },
            version = "2.0.0"
        };

        return Ok(capabilities);
    }

    /// <summary>
    /// Health check endpoint for the AI Assistant
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    [ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult Health()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            service = "KaanAI Assistant"
        });
    }
}