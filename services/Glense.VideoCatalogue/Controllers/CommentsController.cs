using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Glense.VideoCatalogue.Data;
using Glense.VideoCatalogue.Models;
using Glense.VideoCatalogue.DTOs;

namespace Glense.VideoCatalogue.Controllers;

[ApiController]
[Route("api/videos/{videoId:guid}/comments")]
public class CommentsController : ControllerBase
{
    private readonly VideoCatalogueDbContext _db;

    public CommentsController(VideoCatalogueDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetComments(Guid videoId)
    {
        var comments = await _db.Comments
            .Where(c => c.VideoId == videoId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CommentResponseDTO
            {
                Id = c.Id,
                VideoId = c.VideoId,
                UserId = c.UserId,
                Username = c.Username,
                Content = c.Content,
                LikeCount = c.LikeCount,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment(
        Guid videoId,
        [FromBody] CreateCommentRequestDTO dto,
        [FromHeader(Name = "X-User-Id")] Guid userId = default,
        [FromHeader(Name = "X-Username")] string username = "Anonymous")
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var video = await _db.Videos.FindAsync(videoId);
        if (video == null) return NotFound("Video not found");

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            VideoId = videoId,
            UserId = userId,
            Username = username,
            Content = dto.Content,
            LikeCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        var resp = new CommentResponseDTO
        {
            Id = comment.Id,
            VideoId = comment.VideoId,
            UserId = comment.UserId,
            Username = comment.Username,
            Content = comment.Content,
            LikeCount = comment.LikeCount,
            CreatedAt = comment.CreatedAt
        };

        return Created($"/api/videos/{videoId}/comments", resp);
    }

    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid videoId, Guid commentId)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.VideoId == videoId);
        if (comment == null) return NotFound();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
