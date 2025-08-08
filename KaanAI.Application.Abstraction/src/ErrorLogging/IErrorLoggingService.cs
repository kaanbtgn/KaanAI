namespace KaanAI.Application.Abstraction.ErrorLogging;

/// <summary>
/// Service for logging errors to the database
/// </summary>
public interface IErrorLoggingService : IService
{
    /// <summary>
    /// Logs an exception to the ErrorLog table
    /// </summary>
    /// <param name="exception">The exception that occurred</param>
    /// <param name="sessionId">Optional session ID where the error occurred</param>
    /// <returns>The ID of the created error log entry</returns>
    Task<int> LogErrorAsync(Exception exception, int? sessionId = null);
    
    /// <summary>
    /// Logs an error message to the ErrorLog table
    /// </summary>
    /// <param name="message">Error message</param>
    /// <param name="stackTrace">Stack trace (optional)</param>
    /// <param name="sessionId">Optional session ID where the error occurred</param>
    /// <returns>The ID of the created error log entry</returns>
    Task<int> LogErrorAsync(string message, string? stackTrace = null, int? sessionId = null);
}