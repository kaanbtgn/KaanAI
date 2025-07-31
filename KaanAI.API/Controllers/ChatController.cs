using KaanAI.Application.Abstraction.Chat.Contracts;
using KaanAI.Application.Abstraction.Chat;
using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<ChatSessionDto>> CreateSession([FromBody] CreateSessionRequest request)
    {
        try
        {
            var session = await _chatService.CreateSessionAsync(request.CreatedBy);
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

    [HttpGet("sessions/user/{userId}")]
    public async Task<ActionResult<IEnumerable<ChatSessionDto>>> GetUserSessions(string userId)
    {
        try
        {
            var sessions = await _chatService.GetSessionsByUserAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
            return StatusCode(500, "An error occurred while retrieving sessions");
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