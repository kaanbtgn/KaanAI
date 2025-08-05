using KaanAI.Application.Abstraction;
using KaanAI.Application.Abstraction.OpenAi.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OpenAiController : ControllerBase
{
    private readonly IOpenAiService _openAiService;
    private readonly ILogger<OpenAiController> _logger;

    public OpenAiController(IOpenAiService openAiService, ILogger<OpenAiController> logger)
    {
        _openAiService = openAiService;
        _logger = logger;
    }

    [HttpPost("send-message")]
    public async Task<ActionResult<OpenAiResponseDto>> SendMessage([FromBody] SendMessageDto request)
    {
        try
        {
            // OpenAI service now handles all session management automatically
            var aiResponse = await _openAiService.SendMessageAsync(request);
            return Ok(aiResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SendMessage endpoint");
            return StatusCode(500, new OpenAiResponseDto
            {
                IsSuccess = false,
                ErrorMessage = "An error occurred while processing your request",
                SessionId = request.SessionId ?? string.Empty
            });
        }
    }




}