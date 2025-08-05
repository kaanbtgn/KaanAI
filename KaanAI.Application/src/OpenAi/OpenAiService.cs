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

            // Prepare messages for OpenAI with default system message
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("Sen yardımcı bir Türkçe AI asistanısın. Kullanıcıların sorularını net ve anlaşılır bir şekilde yanıtla.")
            };

            // Include conversation history based on request parameter
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
                Temperature = 0.7f // Default temperature - let Azure OpenAI handle max tokens
            };
            
            _logger.LogInformation("Using default temperature: {Temperature}", 
                chatCompletionOptions.Temperature);
            
            _logger.LogInformation("Sending request to Azure OpenAI with {MessageCount} messages", messages.Count);
            
            // Log the messages being sent for debugging
            for (int i = 0; i < messages.Count; i++)
            {
                var msg = messages[i];
                var content = msg switch
                {
                    SystemChatMessage systemMsg => $"SYSTEM: {systemMsg.Content[0].Text}",
                    UserChatMessage userMsg => $"USER: {userMsg.Content[0].Text}",
                    AssistantChatMessage assistantMsg => $"ASSISTANT: {assistantMsg.Content[0].Text}",
                    _ => $"OTHER: {msg.GetType().Name}"
                };
                _logger.LogInformation("Message {Index}: {Content}", i, content.Length > 100 ? content.Substring(0, 100) + "..." : content);
            }
            
            // Create a custom cancellation token with longer timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromMinutes(3)); // 3 minutes timeout
            
            var response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions, cts.Token);
            var chatResponse = response.Value;
            
            _logger.LogInformation("Raw response received from Azure OpenAI");
            _logger.LogInformation("Response content count: {Count}", chatResponse.Content?.Count ?? 0);
            
            // Log token usage information
            if (chatResponse.Usage != null)
            {
                _logger.LogInformation("Token usage - Prompt: {PromptTokens}, Completion: {CompletionTokens}, Total: {TotalTokens}", 
                    chatResponse.Usage.InputTokenCount, 
                    chatResponse.Usage.OutputTokenCount, 
                    chatResponse.Usage.TotalTokenCount);
            }
            else
            {
                _logger.LogWarning("No token usage information available in response");
            }
            
            if (chatResponse.Content == null || chatResponse.Content.Count == 0)
            {
                throw new InvalidOperationException("No content received from Azure OpenAI");
            }
            
            var aiResponseText = chatResponse.Content[0].Text ?? string.Empty;
            
            _logger.LogInformation("Extracted response text length: {Length}", aiResponseText.Length);
            _logger.LogInformation("First 100 chars of response: {Preview}", 
                aiResponseText.Length > 100 ? aiResponseText.Substring(0, 100) : aiResponseText);

            // Extract token usage information
            var promptTokens = chatResponse.Usage?.InputTokenCount ?? 0;
            var completionTokens = chatResponse.Usage?.OutputTokenCount ?? 0;
            var totalTokens = chatResponse.Usage?.TotalTokenCount ?? 0;

            // Add the AI response to the session with token information
            await _chatService.AddAnswerAsync(sessionId, aiResponseText, promptTokens, completionTokens, totalTokens);

            // Extract token usage information
            var tokensUsed = totalTokens;

            var result = new OpenAiResponseDto
            {
                Response = aiResponseText,
                SessionId = sessionId.ToString(),
                IsSuccess = true,
                TokensUsed = tokensUsed,
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
        // Since we always include history now, just call the main method
        return await SendMessageAsync(request, cancellationToken);
    }
}