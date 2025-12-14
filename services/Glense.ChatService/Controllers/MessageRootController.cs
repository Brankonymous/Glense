using Glense.ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Glense.ChatService.Controllers;

[ApiController]
[Authorize]
[Route("api/messages")]
public class MessageRootController : ControllerBase
{
    private readonly IChatService _svc;

    public MessageRootController(IChatService svc)
    {
        _svc = svc;
    }

    [HttpGet("{messageId:guid}")]
    public async Task<IActionResult> GetMessage(Guid messageId)
    {
        var dto = await _svc.GetMessageAsync(messageId, HttpContext.RequestAborted);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpDelete("{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid messageId)
    {
        await _svc.DeleteMessageAsync(messageId, HttpContext.RequestAborted);
        return NoContent();
    }
}
