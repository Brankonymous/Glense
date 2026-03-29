using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    private string GetCurrentUsername()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value
            ?? User.FindFirst("unique_name")?.Value
            ?? "Anonymous";
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
                DislikeCount = c.DislikeCount,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync();

        return Ok(comments);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateComment(
        Guid videoId,
        [FromBody] CreateCommentRequestDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        var username = GetCurrentUsername();

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
            DislikeCount = comment.DislikeCount,
            CreatedAt = comment.CreatedAt
        };

        return Created($"/api/videos/{videoId}/comments", resp);
    }

    [Authorize]
    [HttpPost("{commentId:guid}/like")]
    public async Task<IActionResult> LikeComment(Guid videoId, Guid commentId, [FromBody] CommentLikeRequestDTO dto)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.VideoId == videoId);
        if (comment == null) return NotFound();

        var userId = GetCurrentUserId();
        var existing = await _db.CommentLikes.FirstOrDefaultAsync(cl => cl.UserId == userId && cl.CommentId == commentId);

        if (existing == null)
        {
            _db.CommentLikes.Add(new CommentLike { UserId = userId, CommentId = commentId, IsLiked = dto.IsLiked });
            if (dto.IsLiked) comment.LikeCount++;
            else comment.DislikeCount++;
        }
        else if (existing.IsLiked != dto.IsLiked)
        {
            existing.IsLiked = dto.IsLiked;
            if (dto.IsLiked) { comment.LikeCount++; comment.DislikeCount = Math.Max(0, comment.DislikeCount - 1); }
            else { comment.DislikeCount++; comment.LikeCount = Math.Max(0, comment.LikeCount - 1); }
        }

        await _db.SaveChangesAsync();

        return Ok(new { likeCount = comment.LikeCount, dislikeCount = comment.DislikeCount });
    }

    [Authorize]
    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid videoId, Guid commentId)
    {
        var comment = await _db.Comments.FirstOrDefaultAsync(c => c.Id == commentId && c.VideoId == videoId);
        if (comment == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        if (comment.UserId != currentUserId)
            return Forbid();

        _db.Comments.Remove(comment);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
