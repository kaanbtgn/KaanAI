using KaanAI.Application.Abstraction.Chat.Contracts;
using KaanAI.Application.Abstraction;
using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ApiExplorerSettings(IgnoreApi = true)] // Hide from Swagger - use /api/assistant/chat instead
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(IChatService chatService, ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<ChatSessionDto>> CreateSession()
    {
        try
        {
            var session = await _chatService.CreateSessionAsync();
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating chat session");
            return StatusCode(500, "An error occurred while creating the session");
        }
    }

    [HttpGet("sessions/{id}")]
    public async Task<ActionResult<ChatSessionDetailDto>> GetSession(int id)
    {
        try
        {
            var session = await _chatService.GetSessionAsync(id);
            if (session == null)
                return NotFound();

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving chat session {SessionId}", id);
            return StatusCode(500, "An error occurred while retrieving the session");
        }
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetAllSessions()
    {
        try
        {
            var sessions = await _chatService.GetAllSessionsAsync();
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions");
            return StatusCode(500, "An error occurred while retrieving sessions");
        }
    }

    [HttpGet("sessions/current")]
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

    [HttpPost("questions")]
    public async Task<ActionResult<ChatMessageDto>> AddQuestion([FromBody] AddQuestionRequest request)
    {
        try
        {
            var question = await _chatService.AddQuestionAsync(request.SessionId, request.Content);
            return Ok(question);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding question to session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while adding the question");
        }
    }

    [HttpPost("answers")]
    public async Task<ActionResult<ChatMessageDto>> AddAnswer([FromBody] AddAnswerRequest request)
    {
        try
        {
            var answer = await _chatService.AddAnswerAsync(request.SessionId, request.AnswerText);
            return Ok(answer);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding answer to session {SessionId}", request.SessionId);
            return StatusCode(500, "An error occurred while adding the answer");
        }
    }
}