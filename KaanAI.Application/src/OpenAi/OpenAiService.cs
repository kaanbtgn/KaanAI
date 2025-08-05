using Azure;
using Azure.AI.OpenAI;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.OpenAi.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.ClientModel;
using OpenAI.Chat;

namespace KaanAI.Application;

public class OpenAiService : IOpenAiService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IChatService _chatService;
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAiService> _logger;
    private readonly string _deploymentName;

    public OpenAiService(IUnitOfWork unitOfWork, IChatService chatService, IConfiguration configuration, ILogger<OpenAiService> logger)
    {
        _unitOfWork = unitOfWork;
        _chatService = chatService;
        _logger = logger;
        
        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? 
                      Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? 
                      throw new ArgumentNullException("Azure OpenAI endpoint not configured");
                      
        var apiKey = configuration["AzureOpenAI:APIKey"] ?? 
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? 
                    throw new ArgumentNullException("Azure OpenAI API key not configured");
                    
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? 
                         Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? 
                         throw new ArgumentNullException("Azure OpenAI deployment name not configured");
        
        var apiVersion = configuration["AzureOpenAI:ApiVersion"] ?? "2024-02-15-preview";
        
        _logger.LogInformation("Configuring Azure OpenAI client - Endpoint: {Endpoint}, Deployment: {DeploymentName}, ApiVersion: {ApiVersion}", 
            endpoint, _deploymentName, apiVersion);
        
        try
        {
            // Configure client options with longer timeout
            var clientOptions = new AzureOpenAIClientOptions()
            {
                NetworkTimeout = TimeSpan.FromMinutes(5) // 5 minutes timeout
            };
            
            var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey), clientOptions);
            _chatClient = azureOpenAIClient.GetChatClient(_deploymentName);
            
            _logger.LogInformation("Azure OpenAI client configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Azure OpenAI client");
            throw;
        }
    }

    public async Task<OpenAiResponseDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting OpenAI request with deployment: {DeploymentName}", _deploymentName);
            
            // Get or create current session if no sessionId provided
            int sessionId;
            if (string.IsNullOrEmpty(request.SessionId) || !int.TryParse(request.SessionId, out sessionId))
            {
                var currentSession = await _chatService.GetOrCreateCurrentSessionAsync();
                sessionId = currentSession.Id;
            }

            // Add the question to the session
            await _chatService.AddQuestionAsync(sessionId, request.Message);

            // Prepare messages for OpenAI
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(request.SystemMessage ?? "Sen yardımcı bir AI asistanısın. Kullanıcıların sorularını net ve anlaşılır bir şekilde yanıtla.")
            };

            // Include conversation history if requested
            if (request.IncludeHistory)
            {
                try
                {
                    var sessionDetail = await _chatService.GetSessionAsync(sessionId);
                    if (sessionDetail?.Messages != null)
                    {
                        // Add all previous messages in chronological order (excluding the just-added question)
                        var previousMessages = sessionDetail.Messages
                            .Where(m => m.Content != request.Message || m.Timestamp < DateTime.UtcNow.AddSeconds(-1))
                            .OrderBy(m => m.Timestamp);

                        foreach (var msg in previousMessages)
                        {
                            if (msg.Type == "Question")
                                messages.Add(new UserChatMessage(msg.Content));
                            else if (msg.Type == "Answer")
                                messages.Add(new AssistantChatMessage(msg.Content));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load session history for session {SessionId}", sessionId);
                }
            }

            // Add the current user message
            messages.Add(new UserChatMessage(request.Message));

            var chatCompletionOptions = new ChatCompletionOptions()
            {
                Temperature = request.Temperature ?? 0.7f,
                MaxOutputTokenCount = request.MaxTokens ?? 1000
            };
            
            _logger.LogInformation("Sending request to Azure OpenAI with {MessageCount} messages", messages.Count);
            
            // Create a custom cancellation token with longer timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(3)); // 3 minutes timeout
            
            var response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions, cts.Token);
            var chatResponse = response.Value;
            var aiResponseText = chatResponse.Content[0].Text;

            // Add the AI response to the session
            await _chatService.AddAnswerAsync(sessionId, aiResponseText);

            var result = new OpenAiResponseDto
            {
                Response = aiResponseText,
                SessionId = sessionId.ToString(),
                IsSuccess = true,
                TokensUsed = 0, // TODO: Find correct property for token usage
                Model = _deploymentName,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("OpenAI request successful for session {SessionId}. Tokens used: {TokensUsed}", sessionId, result.TokensUsed);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message to OpenAI");
            return new OpenAiResponseDto
            {
                Response = string.Empty,
                SessionId = request.SessionId ?? "0",
                IsSuccess = false,
                ErrorMessage = ex.Message,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<OpenAiResponseDto> SendMessageWithHistoryAsync(SendMessageDto request, CancellationToken cancellationToken = default)
    {
        // Since SendMessageAsync now handles history by default, we can just call it
        // But ensure IncludeHistory is set to true
        var requestWithHistory = new SendMessageDto
        {
            Message = request.Message,
            SessionId = request.SessionId,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            SystemMessage = request.SystemMessage,
            IncludeHistory = true
        };

        return await SendMessageAsync(requestWithHistory, cancellationToken);
    }
}