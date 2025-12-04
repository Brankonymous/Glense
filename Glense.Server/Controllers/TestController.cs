using Glense.Server;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("test-connection")]
    public IActionResult TestConnection()
    {
        try
        {
            var canConnect = _context.Database.CanConnect();
            return Ok(new { success = canConnect });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
