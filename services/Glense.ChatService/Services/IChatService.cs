using Glense.ChatService.DTOs;

namespace Glense.ChatService.Services;

public interface IChatService
{
    Task<PagedResponse<ChatDto>> GetChatsAsync(Guid? cursor, int pageSize, CancellationToken ct = default);
    Task<ChatDto> CreateChatAsync(CreateChatRequest req, CancellationToken ct = default);
    Task<ChatDto?> GetChatAsync(Guid id, CancellationToken ct = default);
    Task DeleteChatAsync(Guid id, CancellationToken ct = default);

    Task<PagedResponse<MessageDto>> GetMessagesAsync(Guid chatId, Guid? cursor, int pageSize, CancellationToken ct = default);
    Task<MessageDto> CreateMessageAsync(Guid chatId, CreateMessageRequest req, CancellationToken ct = default);
    Task<MessageDto?> GetMessageAsync(Guid messageId, CancellationToken ct = default);
    Task DeleteMessageAsync(Guid messageId, CancellationToken ct = default);
}
