using Glense.ChatService.DTOs;
using Glense.ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Glense.ChatService.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ChatsController : ControllerBase
{
    private readonly IChatService _svc;

    public ChatsController(IChatService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<IActionResult> GetChats([FromQuery] Guid? cursor, [FromQuery] int pageSize = 50)
    {
        var res = await _svc.GetChatsAsync(cursor, pageSize, HttpContext.RequestAborted);
        return Ok(res);
    }

    [HttpPost]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var dto = await _svc.CreateChatAsync(req, HttpContext.RequestAborted);
        return CreatedAtAction(nameof(GetChat), new { chatId = dto.Id }, dto);
    }

    [HttpGet("{chatId:guid}")]
    public async Task<IActionResult> GetChat(Guid chatId)
    {
        var dto = await _svc.GetChatAsync(chatId, HttpContext.RequestAborted);
        if (dto == null) return NotFound();
        return Ok(dto);
    }

    [HttpDelete("{chatId:guid}")]
    public async Task<IActionResult> DeleteChat(Guid chatId)
    {
        await _svc.DeleteChatAsync(chatId, HttpContext.RequestAborted);
        return NoContent();
    }
}
