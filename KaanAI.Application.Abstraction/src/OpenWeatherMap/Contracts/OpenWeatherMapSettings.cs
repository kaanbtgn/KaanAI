namespace KaanAI.Application.Abstraction.OpenWeatherMap.Contracts;

/// <summary>
/// OpenWeatherMap API configuration settings
/// </summary>
public class OpenWeatherMapSettings
{
    public const string SectionName = "OpenWeatherMap";

    /// <summary>
    /// OpenWeatherMap API key
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for OpenWeatherMap API
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openweathermap.org/data/2.5";

    /// <summary>
    /// Default city when no location is specified
    /// </summary>
    public string DefaultCity { get; set; } = "İstanbul";

    /// <summary>
    /// Temperature units (metric, imperial, standard)
    /// </summary>
    public string Units { get; set; } = "metric";

    /// <summary>
    /// Language code for weather descriptions
    /// </summary>
    public string Language { get; set; } = "tr";
}
