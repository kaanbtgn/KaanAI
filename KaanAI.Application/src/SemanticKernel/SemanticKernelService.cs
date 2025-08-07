using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.SemanticKernel;
using KaanAI.Application.Abstraction.SemanticKernel.Contracts;
using KaanAI.Application.Plugins;
using KaanAI.Application.Plugins.GreetingPlugin;
using System.Diagnostics;
using System.Globalization;

namespace KaanAI.Application;

public class SemanticKernelService : ISemanticKernelService
{
    private readonly Kernel _kernel;
    private readonly IChatService _chatService;
    private readonly ILogger<SemanticKernelService> _logger;

    // -----------------------------------------------------------------
    // SYSTEM CONSTRAINT
    // The model only serves 3 topics: weather, stocks, OCR.
    // Other topics are automatically declined.
    // -----------------------------------------------------------------
    private const string SystemPrompt = @"You are a specialized AI assistant that provides services for exactly 3 topics:
1) WEATHER - WeatherPlugin
2) STOCKS - StockMarketPlugin  
3) OCR/TEXT - OcrPlugin

For any questions outside these 3 topics, politely respond with: 'I can only help with weather, stock market, or OCR/text questions.'

Be helpful and professional in your responses.";

    public SemanticKernelService(
        Kernel kernel,
        IChatService chatService,
        ILogger<SemanticKernelService> logger,
        IOpenWeatherMapService weatherService)
    {
        _kernel = kernel;
        _chatService = chatService;
        _logger = logger;

        try
        {
            // ►► Plugin'leri tek seferlik kaydediyoruz. ◄◄
            // Create WeatherPlugin instance manually to avoid circular dependency
            var weatherPlugin = new WeatherPlugin(weatherService, kernel);
            var weatherPluginResult = _kernel.ImportPluginFromObject(weatherPlugin, "WeatherPlugin");
            
            // Create and register GreetingPlugin
            var greetingPlugin = new GreetingPlugin();
            var greetingPluginResult = _kernel.ImportPluginFromObject(greetingPlugin, "GreetingPlugin");
            
            _logger.LogInformation("WeatherPlugin imported successfully. Plugin functions: {Functions}", 
                string.Join(", ", weatherPluginResult.Select(f => f.Name)));
                
            _logger.LogInformation("GreetingPlugin imported successfully. Plugin functions: {Functions}", 
                string.Join(", ", greetingPluginResult.Select(f => f.Name)));
            
            _logger.LogInformation("Available plugins in kernel: {Plugins}", 
                string.Join(", ", _kernel.Plugins.Select(p => $"{p.Name} ({p.Count()} functions)")));
                
            // Verify the plugin was registered correctly
            var registeredPlugin = _kernel.Plugins.FirstOrDefault(p => p.Name == "WeatherPlugin");
            if (registeredPlugin == null)
            {
                _logger.LogError("WeatherPlugin registration failed - plugin not found in collection");
            }
            else
            {
                var getWeatherFunc = registeredPlugin.FirstOrDefault(f => f.Name == "GetWeather");
                if (getWeatherFunc == null)
                {
                    _logger.LogError("GetWeather function not found in WeatherPlugin");
                }
                else
                {
                    _logger.LogInformation("GetWeather function successfully registered");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register WeatherPlugin");
            throw;
        }
        
        // _kernel.ImportPluginFromType<StockMarketPlugin>("StockMarketPlugin"); // TODO aktif değil
        // _kernel.ImportPluginFromType<OcrPlugin>("OcrPlugin");               // TODO aktif değil
    }

    public async Task<SemanticKernelResponseDto> ExecuteAsync(
        SemanticKernelRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            _logger.LogInformation("SK execution started. Msg: {Msg}", request.Message);

            var sessionId = await GetOrCreateSessionIdAsync(request.SessionId);

            await _chatService.AddQuestionAsync(sessionId, request.Message);

            var intent = await DetectIntentAsync(request, cancellationToken);

            var responseText = intent switch
            {
                "weather" => await HandleWeatherAsync(request.Message, cancellationToken),
                "stock"   => "Stock market analysis feature is not yet active.",
                "ocr"     => "OCR feature is not yet active.",
                "greeting" => await HandleGreetingAsync(request.Message, cancellationToken),
                _          => "Merhaba! Ben sadece hava durumu, borsa ve OCR/metin tanıma konularında yardımcı olabilirim. Bu konulardan biri hakkında bir soru sorabilirsiniz."
            };

            // SK OpenAI connector'ından gerçek token kullanımını al (varsa).
            var tokensUsed = responseText.Length / 4; // kaba tahmin

            await _chatService.AddAnswerAsync(
                sessionId,
                responseText,
                promptTokens: tokensUsed,
                completionTokens: 0,
                totalTokens: tokensUsed);

            return new SemanticKernelResponseDto
            {
                Response         = responseText,
                SessionId        = sessionId.ToString(CultureInfo.InvariantCulture),
                IsSuccess        = true,
                TokensUsed       = tokensUsed,
                DetectedIntent   = intent,
                UsedPlugin       = intent switch
                {
                    "weather" => "WeatherPlugin",
                    "greeting" => "GreetingPlugin", 
                    _ => "None"
                },
                CreatedAt        = DateTime.UtcNow,
                ProcessingTime   = sw.Elapsed,
                IntentConfidence = 1.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SK execution error");
            return new SemanticKernelResponseDto
            {
                Response       = "An error occurred, please try again.",
                SessionId      = request.SessionId ?? "0",
                IsSuccess      = false,
                ErrorMessage   = ex.Message,
                CreatedAt      = DateTime.UtcNow,
                ProcessingTime = sw.Elapsed,
                DetectedIntent = "error",
                UsedPlugin     = "ErrorHandler"
            };
        }
    }

    #region Private helpers

    private async Task<string> DetectIntentAsync(
        SemanticKernelRequestDto request,
        CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(request.PreferredPlugin) && 
            request.PreferredPlugin.Trim().ToLowerInvariant() != "string")
            return request.PreferredPlugin.Trim().ToLowerInvariant();

        if (!request.AutoDetectIntent) return "none";

        try
        {
            var classifierPrompt = @"You are a classifier that categorizes user messages. 

Classify the following message into one of these categories:
- weather: questions about weather, temperature, rain, snow, forecast, climate (words like: weather, temperature, rain, snow, forecast, hot, cold, sunny, cloudy, storm, hava durumu, sıcaklık, nasıl, etc.)
- stock: questions about stocks, stock market, trading, shares, investments (words like: stock, market, trading, shares, price, investment, borsa, hisse, etc.)  
- ocr: questions about text recognition, reading text from images, scanning documents (words like: OCR, text recognition, read text, scan, extract text, etc.)
- greeting: greetings, hello, hi, good morning, merhaba, selam, günaydın, etc.
- none: anything else

Respond with ONLY the category name: weather, stock, ocr, greeting, or none

MESSAGE: " + request.Message + @"

CATEGORY:";

            var classificationFunc = _kernel.CreateFunctionFromPrompt(classifierPrompt);

            var result = await _kernel.InvokeAsync(classificationFunc, new KernelArguments(), ct);

            var rawResponse = result.GetValue<string>()?.Trim() ?? "none";
            
            _logger.LogInformation("Intent detection - Input: '{Message}', Raw response: '{RawResponse}'", 
                request.Message, rawResponse);
            
            // Extract just the category name from the response
            var detectedIntent = ExtractCategoryFromResponse(rawResponse).ToLowerInvariant();
            
            _logger.LogInformation("Intent detection - Final detected: '{Intent}'", detectedIntent);
                
            return detectedIntent;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "LLM intent detection failed, falling back to keyword-based detection");
            
            // Fallback to simple keyword detection
            var message = request.Message.ToLowerInvariant();
            
            if (new[] { "hava", "weather", "sıcaklık", "temperature", "nasıl" }.Any(k => message.Contains(k)))
                return "weather";
            if (new[] { "stock", "borsa", "hisse", "market" }.Any(k => message.Contains(k)))
                return "stock";
            if (new[] { "ocr", "text", "read", "scan" }.Any(k => message.Contains(k)))
                return "ocr";
            if (new[] { "merhaba", "hello", "hi", "selam", "günaydın", "iyi akşam", "nasılsın" }.Any(k => message.Contains(k)))
                return "greeting";
                
            return "none";
        }
    }

    private string ExtractCategoryFromResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            return "none";
            
        var text = response.ToLowerInvariant().Trim();
        
        // Look for exact matches first
        var validCategories = new[] { "weather", "stock", "ocr", "greeting", "none" };
        foreach (var category in validCategories)
        {
            if (text == category || text.EndsWith(category) || text.Contains($": {category}"))
                return category;
        }
        
        // Fallback to keyword search
        if (text.Contains("weather") || text.Contains("hava"))
            return "weather";
        if (text.Contains("stock") || text.Contains("borsa"))
            return "stock";
        if (text.Contains("ocr") || text.Contains("text"))
            return "ocr";
        if (text.Contains("greeting") || text.Contains("hello"))
            return "greeting";
            
        return "none";
    }

private async Task<string> HandleWeatherAsync(string message, CancellationToken ct)
{
    try
    {
        // Use direct plugin call with proper Azure OpenAI configuration
        var weatherService = _kernel.Services.GetService<IOpenWeatherMapService>();
        if (weatherService == null)
        {
            return "Hava durumu servisi bulunamadı.";
        }

        var weatherPlugin = new WeatherPlugin(weatherService, _kernel);
        var result = await weatherPlugin.GetWeatherAsync(message, ct);
        
        return result ?? "Hava durumu bilgisi alınamadı.";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting weather for message: {Message}", message);
        return $"Hava durumu bilgisi alınırken bir hata oluştu: {ex.Message}";
    }
}

private async Task<string> HandleGreetingAsync(string message, CancellationToken ct)
{
    try
    {
        var greetingPlugin = new GreetingPlugin();
        var result = await greetingPlugin.GetGreetingAsync(message, ct);
        
        return result ?? "Merhaba! Size nasıl yardımcı olabilirim?";
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error handling greeting for message: {Message}", message);
        return "Merhaba! Ben KaanAI asistanınızım. Size hava durumu, borsa bilgileri ve OCR/metin tanıma konularında yardımcı olabilirim. Nasıl yardımcı olabilirim? 😊";
    }
}

    private async Task<int> GetOrCreateSessionIdAsync(string? sessionId)
    {
        if (int.TryParse(sessionId, out var id)) return id;
        var current = await _chatService.GetOrCreateCurrentSessionAsync();
        return current.Id;
    }

    #endregion
}