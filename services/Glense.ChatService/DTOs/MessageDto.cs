namespace Glense.ChatService.DTOs;

public record MessageDto(Guid Id, Guid ChatId, string Sender, string Content, DateTime CreatedAtUtc);
