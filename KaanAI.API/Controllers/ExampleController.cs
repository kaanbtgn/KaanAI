using KaanAI.Application.Abstraction.Chat;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace KaanAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExampleController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly ILogger<ExampleController> _logger;

        public ExampleController(IChatService chatService, ILogger<ExampleController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var value = Random.Shared.Next(0, 100) * Math.PI;
            _logger.LogDebug("Generated value: {Value}", value);
            return Content(value.ToString());
        }
    }
}
