using KaanAI.Application.Abstraction.Chat.Contracts;
using KaanAI.Application.Abstraction.Chat;
using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<SessionController> _logger;
    private readonly IConfiguration _configuration;

    public SessionController(IChatService chatService, ILogger<SessionController> logger, IConfiguration configuration)
    {
        _chatService = chatService;
        _logger = logger;
        _configuration = configuration;
    }

    [HttpPost]
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var session = await _chatService.CreateSessionAsync(request.CreatedBy);
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, "An error occurred while creating the session");
        }
    }
}