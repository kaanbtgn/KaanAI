using System.ComponentModel.DataAnnotations;
using KaanAI.Application.Abstraction.SemanticKernel.Contracts;

namespace KaanAI.Application.Abstraction.OpenWeatherMap.Contracts;

public class WeatherRequestDto : SemanticKernelRequestDto
{
    [Required]
    public string Location { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of days for forecast (1-7)
    /// </summary>
    public int Days { get; set; } = 1;
    
    /// <summary>
    /// Include additional weather details (humidity, wind, etc.)
    /// </summary>
    public bool IncludeDetails { get; set; } = true;
}
