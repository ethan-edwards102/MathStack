using Microsoft.AspNetCore.Mvc;

namespace MathApis.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "ok0000",
            service = "MathAPI"
        });
    }
}