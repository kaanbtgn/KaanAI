using Azure;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.Chat;
using KaanAI.Application.Abstraction.OpenAi;
using KaanAI.Application.Abstraction.OpenAi.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
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
                      
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? 
                    Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY") ?? 
                    throw new ArgumentNullException("Azure OpenAI API key not configured");
                    
        _deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? 
                         Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? 
                         throw new ArgumentNullException("Azure OpenAI deployment name not configured");
        
        var openAIClient = new OpenAIClient(new AzureKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        });
        
        _chatClient = openAIClient.GetChatClient(_deploymentName);
    }

    public async Task<OpenAiResponseDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Get or create session automatically
            int sessionId;
            if (!string.IsNullOrEmpty(request.SessionId) && int.TryParse(request.SessionId, out sessionId))
            {
                // Use provided session ID
                var existingSession = await _chatService.GetSessionAsync(sessionId);
                if (existingSession == null)
                {
                    // Session doesn't exist, create a new one
                    var newSession = await _chatService.CreateSessionAsync();
                    sessionId = newSession.Id;
                    _logger.LogWarning("Provided session {ProvidedSessionId} not found, created new session {NewSessionId}", request.SessionId, sessionId);
                }
            }
            else
            {
                // No session provided, get or create current session
                var currentSession = await _chatService.GetOrCreateCurrentSessionAsync();
                sessionId = currentSession.Id;
            }

            // 2. Build messages including history if requested
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("Sen yardımcı bir AI asistanısın. Kullanıcıların sorularını net ve anlaşılır bir şekilde yanıtla. Geçmiş konuşma bağlamını dikkate al.")
            };

            // Include conversation history if requested
            if (request.IncludeHistory)
            {
                try
                {
                    var questions = await _unitOfWork.Repository<Domain.Question>().FindAsync(q => q.SessionId == sessionId);
                    var answers = await _unitOfWork.Repository<Domain.Answer>().FindAsync(a => a.SessionId == sessionId);
                    
                    var allHistory = new List<(DateTime timestamp, string content, string type)>();
                    
                    foreach (var q in questions)
                        allHistory.Add((q.AskedAt, q.Content, "user"));
                    
                    foreach (var a in answers)
                        allHistory.Add((a.AnsweredAt, a.AnswerText, "assistant"));
                    
                    foreach (var item in allHistory.OrderBy(x => x.timestamp))
                    {
                        if (item.type == "user")
                            messages.Add(new UserChatMessage(item.content));
                        else
                            messages.Add(new AssistantChatMessage(item.content));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load session history for session {SessionId}", sessionId);
                    // Continue without history if loading fails
                }
            }

            // Add current message
            messages.Add(new UserChatMessage(request.Message));

            // 3. Send to OpenAI
            var chatCompletionOptions = new ChatCompletionOptions()
            {
                Temperature = 0.7f
            };
            
            var response = await _chatClient.CompleteChatAsync(messages, chatCompletionOptions, cancellationToken);
            var chatResponse = response.Value;

            var aiResponseText = chatResponse.Content[0].Text;

            // 4. Save to database
            await _chatService.AddQuestionAsync(sessionId, request.Message);
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
            
            // Try to get session ID for error response
            var errorSessionId = "unknown";
            if (!string.IsNullOrEmpty(request.SessionId))
                errorSessionId = request.SessionId;
            else
            {
                try
                {
                    var currentSession = await _chatService.GetOrCreateCurrentSessionAsync();
                    errorSessionId = currentSession.Id.ToString();
                }
                catch
                {
                    // Ignore errors when getting session for error response
                }
            }
            
            return new OpenAiResponseDto
            {
                Response = string.Empty,
                SessionId = errorSessionId,
                IsSuccess = false,
                ErrorMessage = ex.Message,
                CreatedAt = DateTime.UtcNow
            };
        }
    }


}