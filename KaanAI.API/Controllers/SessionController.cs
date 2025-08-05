using KaanAI.Application.Abstraction.Chat.Contracts;
using KaanAI.Application.Abstraction;
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
    public async Task<ActionResult<ChatSessionDto>> CreateSession()
    {
        try
        {
            var session = await _chatService.CreateSessionAsync();
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, "An error occurred while creating the session");
        }
    }

    [HttpGet("current")]
    public async Task<ActionResult<ChatSessionDto>> GetOrCreateCurrentSession()
    {
        try
        {
            var session = await _chatService.GetOrCreateCurrentSessionAsync();
            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating current session");
            return StatusCode(500, "An error occurred while getting the current session");
        }
    }
}