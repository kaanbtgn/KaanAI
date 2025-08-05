using Azure.AI.OpenAI;
using Azure;
using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.OpenAI;
using KaanAI.Application.Abstraction.OpenAI.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace KaanAI.Application;

public class OpenAiServiceService : IOpenAIService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly OpenAIClient _openAIClient;
    private readonly ILogger<OpenAiServiceService> _logger;
    private readonly string _deploymentName;

    public OpenAiServiceService(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<OpenAiServiceService> logger)
    {
        _unitOfWork = unitOfWork;
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
        
        _openAIClient = new OpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
    }

    public async Task<OpenAIResponseDto> SendMessageAsync(SendMessageDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _deploymentName,
                Messages =
                {
                    new ChatRequestSystemMessage("Sen yardımcı bir AI asistanısın. Kullanıcıların sorularını net ve anlaşılır bir şekilde yanıtla."),
                    new ChatRequestUserMessage(request.Message)
                },
                MaxTokens = request.MaxTokens ?? 1000,
                Temperature = request.Temperature ?? 0.7f
            };

            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
            var chatResponse = response.Value;

            var result = new OpenAIResponseDto
            {
                Response = chatResponse.Choices[0].Message.Content,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                IsSuccess = true,
                TokensUsed = chatResponse.Usage?.TotalTokens ?? 0,
                Model = _deploymentName,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("OpenAI request successful. Tokens used: {TokensUsed}", result.TokensUsed);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message to OpenAI");
            return new OpenAIResponseDto
            {
                Response = string.Empty,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                IsSuccess = false,
                ErrorMessage = ex.Message,
                CreatedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<OpenAIResponseDto> SendMessageWithHistoryAsync(SendMessageDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var messages = new List<ChatRequestMessage>
            {
                new ChatRequestSystemMessage("Sen yardımcı bir AI asistanısın. Kullanıcıların sorularını net ve anlaşılır bir şekilde yanıtla. Geçmiş konuşma bağlamını dikkate al.")
            };

            // Eğer session history varsa, geçmiş mesajları ekle
            if (!string.IsNullOrEmpty(request.SessionId))
            {
                // TODO: Veritabanından session history'yi getir ve messages listesine ekle
                // var sessionHistory = await _unitOfWork.ChatRepository.GetSessionHistoryAsync(request.SessionId);
                // foreach (var message in sessionHistory)
                // {
                //     messages.Add(new ChatRequestUserMessage(message.Question));
                //     messages.Add(new ChatRequestAssistantMessage(message.Answer));
                // }
            }

            messages.Add(new ChatRequestUserMessage(request.Message));

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = _deploymentName,
                MaxTokens = request.MaxTokens ?? 1000,
                Temperature = request.Temperature ?? 0.7f
            };

            foreach (var message in messages)
            {
                chatCompletionsOptions.Messages.Add(message);
            }
            
            var response = await _openAIClient.GetChatCompletionsAsync(chatCompletionsOptions, cancellationToken);
            var chatResponse = response.Value;

            var result = new OpenAIResponseDto
            {
                Response = chatResponse.Choices[0].Message.Content,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                IsSuccess = true,
                TokensUsed = chatResponse.Usage?.TotalTokens ?? 0,
                Model = _deploymentName,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("OpenAI request with history successful. Tokens used: {TokensUsed}", result.TokensUsed);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while sending message with history to OpenAI");
            return new OpenAIResponseDto
            {
                Response = string.Empty,
                SessionId = request.SessionId ?? Guid.NewGuid().ToString(),
                IsSuccess = false,
                ErrorMessage = ex.Message,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}