using KaanAI.Application.Abstraction.OpenWeatherMap.Contracts;

namespace KaanAI.Application.Abstraction;


/// <summary>
/// Service for interacting with OpenWeatherMap API
/// </summary>
public interface IOpenWeatherMapService : IService
{
    Task<string> GetCurrentWeatherAsync(string location, CancellationToken cancellationToken = default);
    Task<string> GetForecastAsync(string location, int days = 5, CancellationToken cancellationToken = default);
    Task<OpenWeatherMapResponse?> GetCurrentWeatherDataAsync(string location, CancellationToken cancellationToken = default);
}