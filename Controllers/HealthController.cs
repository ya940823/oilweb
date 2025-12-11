using Microsoft.AspNetCore.Mvc;

namespace OilWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check endpoint called");
        
        return Ok(new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "OilWeb API",
            Version = "1.0.0"
        });
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { Message = "pong" });
    }
}
