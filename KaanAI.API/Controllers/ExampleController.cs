using Microsoft.AspNetCore.Mvc;

namespace KaanAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExampleController : ControllerBase
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            var value = Random.Shared.Next(0, 100) * Math.PI;
            return Content(value.ToString());
        }
    }
}
