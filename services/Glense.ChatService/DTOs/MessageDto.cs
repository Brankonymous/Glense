namespace Glense.ChatService.DTOs;

public record MessageDto(Guid Id, Guid ChatId, Guid UserId, string Username, string Sender, string Content, DateTime CreatedAtUtc);
