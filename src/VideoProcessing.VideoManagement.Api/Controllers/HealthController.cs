using Microsoft.AspNetCore.Mvc;

namespace VideoProcessing.VideoManagement.Api.Controllers;

[ApiController]
[Route("[controller]")] // A story pede /health, então vou ajustar na rota ou no atributo
public class HealthController : ControllerBase
{
    [HttpGet]
    [Route("/health")] // Garante que a rota seja exatamente /health, ignorando o prefixo da classe se necessario, ou uso [Route("health")] se a base for /
    public IActionResult Get()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow 
        });
    }
}
