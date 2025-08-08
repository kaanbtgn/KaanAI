using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.ErrorLogging;
using KaanAI.Domain;

namespace KaanAI.Application.ErrorLogging;

/// <summary>
/// Service for logging errors to the database
/// </summary>
public class ErrorLoggingService : IErrorLoggingService
{
    private readonly IUnitOfWork _unitOfWork;

    public ErrorLoggingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<int> LogErrorAsync(Exception exception, int? sessionId = null)
    {
        return await LogErrorAsync(exception.Message, exception.StackTrace, sessionId);
    }

    public async Task<int> LogErrorAsync(string message, string? stackTrace = null, int? sessionId = null)
    {
        var errorLog = new ErrorLog
        {
            Message = TruncateMessage(message),
            StackTrace = TruncateStackTrace(stackTrace) ?? string.Empty,
            SessionId = sessionId ?? 0, // Default to 0 if no session
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Repository<ErrorLog>().AddAsync(errorLog);
        await _unitOfWork.SaveChangesAsync();

        return errorLog.Id;
    }

    private static string TruncateMessage(string message)
    {
        // Ensure message fits within the 4000 character limit from database
        return string.IsNullOrEmpty(message) ? "Unknown error" : 
               message.Length > 4000 ? message.Substring(0, 4000) : message;
    }

    private static string? TruncateStackTrace(string? stackTrace)
    {
        // Ensure stack trace fits within the 8000 character limit from database
        return string.IsNullOrEmpty(stackTrace) ? null :
               stackTrace.Length > 8000 ? stackTrace.Substring(0, 8000) : stackTrace;
    }
}