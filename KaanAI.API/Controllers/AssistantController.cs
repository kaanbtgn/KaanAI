using KaanAI.Application.Abstraction.SemanticKernel;
using KaanAI.Application.Abstraction.SemanticKernel.Contracts;
using KaanAI.Application.Abstraction;
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
    private readonly ILogger<AssistantController> _logger;

    public AssistantController(
        ISemanticKernelService semanticKernelService,
        ILogger<AssistantController> logger)
    {
        _semanticKernelService = semanticKernelService;
        _logger = logger;
    }
/// <summary>
/// AI Assistant – Weather / Stock / OCR dışındaki konuları kibarca reddeder.
/// </summary>
[HttpPost("chat")]
[ProducesResponseType(typeof(SemanticKernelResponseDto), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
public async Task<ActionResult> Chat([FromBody] SemanticKernelRequestDto request)
{
    try
    {
        _logger.LogInformation("Assistant request: {Message}", request.Message);

        var response = await _semanticKernelService.ExecuteAsync(request);
        return Ok(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Assistant error");
        return StatusCode(500, new
        {
            response    = "Üzgünüm, isteğinizi işlerken bir hata oluştu.",
            isSuccess   = false,
            errorMessage= ex.Message,
            createdAt   = DateTime.UtcNow
        });
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
            },
            features = new[]
            {
                "Intelligent plugin detection using LLM",
                "Multi-language support (Turkish & English)",
                "Session management",
                "Context-aware responses",
                "Real-time crypto data via Binance API (free)",
                "Weather data via OpenWeatherMap",
                "AI-powered analysis via Azure OpenAI"
            },
            supportedCryptos = new[]
            {
                "BTC", "ETH", "ADA", "DOGE", "LTC", "XRP", "BNB", "SOL", "MATIC", "AVAX"
            },
            endpoints = new
            {
                chat = "/api/Assistant/chat",
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