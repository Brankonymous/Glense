namespace Glense.ChatService.DTOs;

public record MessageDto(Guid Id, Guid ChatId, Guid UserId, string Sender, string Content, DateTime CreatedAtUtc);
