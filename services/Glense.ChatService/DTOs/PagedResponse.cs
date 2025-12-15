namespace Glense.ChatService.DTOs;

public record PagedResponse<T>(IEnumerable<T> Items, Guid? NextCursor);
