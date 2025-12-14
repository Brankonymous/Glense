using Glense.ChatService.DTOs;
using Glense.ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Glense.ChatService.Controllers;

[ApiController]
[Authorize]
[Route("api/chats/{chatId:guid}/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IChatService _svc;

    public MessagesController(IChatService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(Guid chatId, [FromQuery] Guid? cursor, [FromQuery] int pageSize = 50)
    {
        var res = await _svc.GetMessagesAsync(chatId, cursor, pageSize, HttpContext.RequestAborted);
        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> CreateMessage(Guid chatId, [FromBody] CreateMessageRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var dto = await _svc.CreateMessageAsync(chatId, req, HttpContext.RequestAborted);
            // Return Location header pointing to GET /api/messages/{messageId}
            return CreatedAtAction(nameof(MessageRootController.GetMessage), "MessageRoot", new { messageId = dto.Id }, dto);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
