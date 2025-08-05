using System.Text;

namespace KaanAI.API.Middleware;

public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for static files and health checks
        if (context.Request.Path.StartsWithSegments("/swagger") ||
            context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.Value?.Contains(".") == true)
        {
            await _next(context);
            return;
        }

        var startTime = DateTime.UtcNow;

        // Log request
        await LogRequest(context);

        // Capture response
        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Request failed for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            throw;
        }
        finally
        {
            var duration = DateTime.UtcNow - startTime;
            
            // Log response
            await LogResponse(context, duration);

            // Copy response back to original stream
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
    }

    private async Task LogRequest(HttpContext context)
    {
        var request = context.Request;
        
        _logger.LogInformation("Request {Method} {Path} from {RemoteIp} - UserAgent: {UserAgent}",
            request.Method,
            request.Path,
            context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
            request.Headers.UserAgent.ToString());

        // Log request body for POST/PUT requests to OpenAI endpoints
        if ((request.Method == "POST" || request.Method == "PUT") && 
            request.Path.StartsWithSegments("/api/OpenAi"))
        {
            request.EnableBuffering();
            var requestBody = await ReadStreamAsync(request.Body);
            request.Body.Position = 0;

            if (!string.IsNullOrEmpty(requestBody))
            {
                // Mask sensitive data
                var maskedBody = MaskSensitiveData(requestBody);
                _logger.LogInformation("Request Body: {RequestBody}", maskedBody);
            }
        }
    }

    private async Task LogResponse(HttpContext context, TimeSpan duration)
    {
        var response = context.Response;
        
        _logger.LogInformation("Response {StatusCode} for {Method} {Path} - Duration: {Duration}ms",
            response.StatusCode,
            context.Request.Method,
            context.Request.Path,
            duration.TotalMilliseconds);

        // Log response body for OpenAI endpoints (truncated)
        if (context.Request.Path.StartsWithSegments("/api/OpenAi") && response.Body.CanSeek)
        {
            response.Body.Seek(0, SeekOrigin.Begin);
            var responseBody = await ReadStreamAsync(response.Body);
            response.Body.Seek(0, SeekOrigin.Begin);

            if (!string.IsNullOrEmpty(responseBody))
            {
                // Truncate long responses
                var truncatedBody = responseBody.Length > 500 
                    ? responseBody.Substring(0, 500) + "..." 
                    : responseBody;
                    
                _logger.LogInformation("Response Body: {ResponseBody}", truncatedBody);
            }
        }
    }

    private static async Task<string> ReadStreamAsync(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        return await reader.ReadToEndAsync();
    }

    private static string MaskSensitiveData(string data)
    {
        // Mask API keys and other sensitive information
        return data.Replace("\"APIKey\":", "\"APIKey\": \"***MASKED***\",")
                  .Replace("\"apiKey\":", "\"apiKey\": \"***MASKED***\",");
    }
}
