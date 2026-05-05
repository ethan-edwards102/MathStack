using Microsoft.AspNetCore.Mvc;

namespace MathApiClient.Controllers;

public class HealthController : Controller
{
    [HttpGet("/health")]
    public IActionResult Index()
    {
        return Content("ok");
    }
}