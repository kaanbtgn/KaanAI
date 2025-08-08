using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using KaanAI.Application.Abstraction.OpenWeatherMap.Contracts;
using KaanAI.Application.Abstraction;

namespace KaanAI.Application;

/// <summary>
/// Service for interacting with OpenWeatherMap API
/// </summary>
public class OpenWeatherMapService : IOpenWeatherMapService
{
    private readonly HttpClient _httpClient;
    private readonly OpenWeatherMapSettings _settings;
    private readonly ILogger<OpenWeatherMapService> _logger;

    public OpenWeatherMapService(
        HttpClient httpClient,
        IOptions<OpenWeatherMapSettings> settings,
        ILogger<OpenWeatherMapService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        
        // Ensure BaseAddress is set as fallback if not set in Program.cs
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri($"{_settings.BaseUrl}/data/2.5/");
            _logger.LogInformation("Set HttpClient BaseAddress to: {BaseAddress}", _httpClient.BaseAddress);
        }
        else
        {
            _logger.LogInformation("HttpClient BaseAddress already set to: {BaseAddress}", _httpClient.BaseAddress);
        }
    }

    public async Task<string> GetCurrentWeatherAsync(string location, CancellationToken cancellationToken = default)
    {
        try
        {
            var weatherData = await GetCurrentWeatherDataAsync(location, cancellationToken);
            if (weatherData == null)
            {
                return $"Hava durumu bilgisi bulunamadı: {location}";
            }

            return FormatWeatherData(weatherData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current weather for {Location}", location);
            return $"Hava durumu bilgisi alınırken hata oluştu: {location}";
        }
    }

    public async Task<string> GetForecastAsync(string location, int days = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            days = Math.Min(Math.Max(days, 1), 5); // OpenWeatherMap free tier allows max 5 days
            
            var url = $"forecast?q={Uri.EscapeDataString(location)}&appid={_settings.ApiKey}&units={_settings.Units}&lang={_settings.Language}";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var forecast = JsonSerializer.Deserialize<OpenWeatherMapForecastResponse>(jsonContent);
            
            if (forecast?.List == null || !forecast.List.Any())
            {
                return $"Hava durumu tahmini bulunamadı: {location}";
            }

            return FormatForecastData(forecast, days);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weather forecast for {Location}", location);
            return $"Hava durumu tahmini alınırken hata oluştu: {location}";
        }
    }

    public async Task<OpenWeatherMapResponse?> GetCurrentWeatherDataAsync(string location, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ApiKey))
            {
                _logger.LogWarning("OpenWeatherMap API key is not configured");
                return null;
            }

            var url = $"weather?q={Uri.EscapeDataString(location)}&appid={_settings.ApiKey}&units={_settings.Units}&lang={_settings.Language}";
            
            _logger.LogInformation("Making request to OpenWeatherMap API for location: '{Location}' using URL: {Url}", location, url);
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("OpenWeatherMap API request failed with status: {StatusCode}, Response: {ErrorContent}, Location: '{Location}'", 
                    response.StatusCode, errorContent, location);
                return null;
            }
            
            var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherData = JsonSerializer.Deserialize<OpenWeatherMapResponse>(jsonContent);
            
            _logger.LogInformation("Successfully retrieved weather data for: {Location}", location);
            return weatherData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving weather data for {Location}", location);
            return null;
        }
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
        var visibility = Math.Round(weather.Visibility / 1000.0, 1); // Convert m to km

        var result = $"🌤️ **{location} Hava Durumu**\n\n";
        result += $"🌡️ **Sıcaklık:** {temp}°C (Hissedilen: {feelsLike}°C)\n";
        result += $"☁️ **Durum:** {char.ToUpper(description[0])}{description[1..]}\n";
        result += $"💧 **Nem:** %{humidity}\n";
        result += $"🌬️ **Rüzgar:** {windSpeed} km/s\n";
        result += $"📊 **Basınç:** {pressure} hPa\n";
        result += $"👁️ **Görüş:** {visibility} km\n";

        // Add sunrise and sunset if available
        if (weather.Sys != null)
        {
            var sunrise = DateTimeOffset.FromUnixTimeSeconds(weather.Sys.Sunrise).ToLocalTime();
            var sunset = DateTimeOffset.FromUnixTimeSeconds(weather.Sys.Sunset).ToLocalTime();
            result += $"🌅 **Güneş Doğuşu:** {sunrise:HH:mm}\n";
            result += $"🌇 **Güneş Batışı:** {sunset:HH:mm}\n";
        }

        // Add update time
        var dateTime = DateTimeOffset.FromUnixTimeSeconds(weather.Dt).ToLocalTime();
        result += $"🕐 **Güncelleme:** {dateTime:dd.MM.yyyy HH:mm}";

        return result;
    }

    private string FormatForecastData(OpenWeatherMapForecastResponse forecast, int days)
    {
        var location = $"{forecast.City?.Name}, {forecast.City?.Country}";
        var result = $"🌦️ **{location} {days} Günlük Tahmin**\n\n";

        var forecastsByDate = forecast.List?
            .Take(days * 8) // 8 forecasts per day (every 3 hours)
            .GroupBy(f => DateTimeOffset.FromUnixTimeSeconds(f.Dt).Date)
            .Take(days)
            .ToList();

        if (forecastsByDate == null || !forecastsByDate.Any())
        {
            return "Hava durumu tahmini bulunamadı.";
        }

        foreach (var dayGroup in forecastsByDate)
        {
            var date = dayGroup.Key;
            var dayForecasts = dayGroup.ToList();
            
            // Find the forecast closest to noon for the main temperature
            var noonForecast = dayForecasts
                .OrderBy(f => Math.Abs(DateTimeOffset.FromUnixTimeSeconds(f.Dt).Hour - 12))
                .First();

            var minTemp = Math.Round(dayForecasts.Min(f => f.Main?.TempMin ?? 0), 1);
            var maxTemp = Math.Round(dayForecasts.Max(f => f.Main?.TempMax ?? 0), 1);
            var description = noonForecast.Weather?.FirstOrDefault()?.Description ?? "Bilinmiyor";
            var humidity = noonForecast.Main?.Humidity ?? 0;
            var windSpeed = Math.Round((noonForecast.Wind?.Speed ?? 0) * 3.6, 1);

            var dayName = date.ToString("dddd", new System.Globalization.CultureInfo("tr-TR"));
            result += $"📅 **{dayName} ({date:dd.MM})**\n";
            result += $"🌡️ En düşük: {minTemp}°C / En yüksek: {maxTemp}°C\n";
            result += $"☁️ Durum: {char.ToUpper(description[0])}{description[1..]}\n";
            result += $"💧 Nem: %{humidity} | 🌬️ Rüzgar: {windSpeed} km/s\n\n";
        }

        return result.TrimEnd();
    }
}
