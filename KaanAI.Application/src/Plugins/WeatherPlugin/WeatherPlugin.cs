using Microsoft.SemanticKernel;
using System.ComponentModel;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.OpenWeatherMap.Contracts;

namespace KaanAI.Application.Plugins;

public class WeatherPlugin
{
    private readonly IOpenWeatherMapService _weatherService;
    private readonly Kernel _kernel;

    public WeatherPlugin(IOpenWeatherMapService weatherService, Kernel kernel)
    {
        _weatherService = weatherService;
        _kernel = kernel;
    }

    [KernelFunction]
    [Description("Get current weather for a specific location with AI-generated suggestions")]
    public async Task<string> GetWeatherAsync(
        [Description("City name or user message about weather")] string input,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Extract the actual city name from the message using LLM
        var extractedLocation = await ExtractLocationFromMessage(input, cancellationToken);
        
        // Step 2: Get weather data from OpenWeatherMap API
        var weatherData = await _weatherService.GetCurrentWeatherDataAsync(extractedLocation);
        
        if (weatherData == null)
        {
            // Try with fallback location if the extracted location fails
            if (extractedLocation != "Istanbul")
            {
                weatherData = await _weatherService.GetCurrentWeatherDataAsync("Istanbul");
            }
            
            if (weatherData == null)
            {
                return $"Hava durumu bilgisi alınamadı. Lütfen geçerli bir şehir adı girin. Girilen konum: '{extractedLocation}'";
            }
        }

        var weatherInfo = FormatWeatherData(weatherData);
        
        // Step 3: Use LLM to analyze weather conditions and generate suggestions
        var analysisPrompt = @"Sen bir hava durumu uzmanısın. Aşağıdaki hava durumu bilgisini analiz et ve kullanıcıya:
            1. Hava durumu hakkında genel bilgi ver
            2. Nasıl giyinmesi gerektiği konusunda önerilerde bulun
            3. Hangi aktiviteleri yapabileceği hakkında öneriler ver
            4. Dikkat etmesi gereken konuları belirt
            Hava Durumu Verileri: " + weatherInfo + @"
            
            Samimi ve yardımcı bir dille, emoji kullanarak yanıtla.";

        var analysisFunction = _kernel.CreateFunctionFromPrompt(analysisPrompt);

        var analysisResult = await _kernel.InvokeAsync(
            analysisFunction, 
            new KernelArguments(),
            cancellationToken);
        
        var analysis = analysisResult.GetValue<string>() ?? "Analiz yapılamadı.";
        
        return $"{weatherInfo}\n\n🤖 **Hava Durumu Analizi:**\n{analysis}";
    }

    [KernelFunction("get_forecast")]
    [Description("Get weather forecast for multiple days")]
    public async Task<string> GetForecastAsync(
        [Description("City name (e.g., 'Istanbul', 'Ankara', 'London')")] string location,
        [Description("Number of days for forecast (1-5)")] int days = 3)
    {
        // Limit days to 5 (OpenWeatherMap free tier limitation)
        days = Math.Min(Math.Max(days, 1), 5);
        return await _weatherService.GetForecastAsync(location, days);
    }

    [KernelFunction("get_detailed_weather")]
    [Description("Get detailed current weather information without suggestions")]
    public async Task<string> GetDetailedWeatherAsync(
        [Description("City name (e.g., 'Istanbul', 'Ankara', 'London')")] string location)
    {
        return await _weatherService.GetCurrentWeatherAsync(location);
    }

    private string FormatWeatherData(OpenWeatherMapResponse weather)
    {
        var location = $"{weather.Name}, {weather.Sys?.Country}";
        var temp = Math.Round(weather.Main?.Temp ?? 0, 1);
        var feelsLike = Math.Round(weather.Main?.FeelsLike ?? 0, 1);
        var description = weather.Weather?.FirstOrDefault()?.Description ?? "Bilinmiyor";
        var humidity = weather.Main?.Humidity ?? 0;
        var windSpeed = Math.Round((weather.Wind?.Speed ?? 0) * 3.6, 1); // Convert m/s to km/h
        var pressure = weather.Main?.Pressure ?? 0;

        var result = $"🌤️ **{location} Hava Durumu**\n\n";
        result += $"🌡️ **Sıcaklık:** {temp}°C (Hissedilen: {feelsLike}°C)\n";
        result += $"☁️ **Durum:** {char.ToUpper(description[0])}{description[1..]}\n";
        result += $"💧 **Nem:** %{humidity}\n";
        result += $"🌬️ **Rüzgar:** {windSpeed} km/s\n";
        result += $"📊 **Basınç:** {pressure} hPa\n";

        // Add time information
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(weather.Dt).ToLocalTime();
        result += $"🕐 **Güncelleme:** {dateTime:dd.MM.yyyy HH:mm}";

        return result;
    }

    private async Task<string> ExtractLocationFromMessage(string message, CancellationToken ct)
    {
        // If the message is already a simple city name, return it
        if (!message.Contains(" ") && !message.Contains("?") && !message.Contains("'"))
        {
            return message;
        }

        // Extract city name from weather-related message using LLM
        var extractLocationPrompt = @"
            Extract the city name from this Turkish or English message about weather.
            If no specific location is mentioned, return 'Istanbul'.
            Return ONLY the city name in English without any extra text, punctuation, or quotes.
            Use standard English city names that are recognized by international weather APIs.
            
            Rules:
            - İstanbul -> Istanbul
            - İzmir -> Izmir
            - If you see Turkish characters, convert to English equivalents
            - Never return special characters or punctuation
            - Always return a valid, internationally recognized city name
            
            Examples:
            - 'İstanbul'da hava nasıl?' -> Istanbul
            - 'Ankara hava durumu' -> Ankara  
            - 'How is the weather in London?' -> London
            - 'Bugün hava nasıl?' -> Istanbul
            - 'What's the weather like?' -> Istanbul
            
            Message: " + message + @"
            City:";

        var extractLocationFunction = _kernel.CreateFunctionFromPrompt(extractLocationPrompt);

        var locationResult = await _kernel.InvokeAsync(
            extractLocationFunction,
            new KernelArguments(),
            ct);
        
        return locationResult.GetValue<string>()?.Trim() ?? "Istanbul";
    }
}