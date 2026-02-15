using Microsoft.AspNetCore.Mvc;

namespace VideoProcessing.VideoManagement.Api.Controllers;

/// <summary>
/// Health check: rota GET /health está mapeada como minimal API no Program.cs para evitar 404
/// com PathBase no Lambda (GatewayPathBaseMiddleware). Este controller mantém o contrato da story.
/// </summary>
[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
