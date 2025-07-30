using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers;
[ApiController]
[Route("[controller]")]
public class SessionController : Controller
{
    private readonly ILogger<SessionController> _logger;
    private readonly IConfiguration _configuration;
    

    public SessionController(ILogger<SessionController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        var a = _configuration?["Services;AI:OpenAI:EndPoint"];
    }

    [HttpPost]
    public IActionResult CreateSession()
    {
        return Ok();
    }
}