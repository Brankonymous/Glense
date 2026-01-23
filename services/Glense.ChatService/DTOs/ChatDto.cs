namespace Glense.ChatService.DTOs;

public record ChatDto(Guid Id, string Topic, DateTime CreatedAtUtc, int MessagesCount);
