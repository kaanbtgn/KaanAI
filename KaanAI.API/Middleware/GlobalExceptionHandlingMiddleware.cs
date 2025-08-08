using KaanAI.Application.Abstraction.ErrorLogging;
using System.Net;
using System.Text.Json;

namespace KaanAI.API.Middleware;

/// <summary>
/// Global exception handling middleware that catches all unhandled exceptions,
/// logs them to the database, and returns consistent error responses.
/// </summary>
public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IErrorLoggingService errorLoggingService)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred for {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            // Try to extract session ID from request
            int? sessionId = ExtractSessionId(context);

            // Log error to database
            try
            {
                await errorLoggingService.LogErrorAsync(ex, sessionId);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to log error to database");
            }

            // Return consistent error response
            await HandleExceptionAsync(context, ex);
        }
    }

    private static int? ExtractSessionId(HttpContext context)
    {
        try
        {
            // Check query parameters first
            if (context.Request.Query.TryGetValue("sessionId", out var sessionIdQuery) &&
                int.TryParse(sessionIdQuery.FirstOrDefault(), out var sessionId))
            {
                return sessionId;
            }

            // Check headers
            if (context.Request.Headers.TryGetValue("SessionId", out var sessionIdHeader) &&
                int.TryParse(sessionIdHeader.FirstOrDefault(), out var headerSessionId))
            {
                return headerSessionId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            isSuccess = false,
            errorMessage = "An internal server error occurred. Please try again later.",
            timestamp = DateTime.UtcNow,
            // Only expose exception details in development
            details = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
                ? exception.Message 
                : null
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}