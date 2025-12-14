using Glense.ChatService.Data;
using Glense.ChatService.DTOs;
using Glense.ChatService.Models;
using Microsoft.EntityFrameworkCore;

namespace Glense.ChatService.Services;

public class ChatService : IChatService
{
    private readonly ChatDbContext _db;

    public ChatService(ChatDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResponse<ChatDto>> GetChatsAsync(Guid? cursor, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
    IQueryable<Models.Chat> query = _db.Chats.AsNoTracking().OrderBy(c => c.CreatedAtUtc).ThenBy(c => c.Id);
        if (cursor is not null)
        {
            // cursor is last seen createdAt + id tie-breaker - but we only have id cursor here; simple keyset by createdAt via lookup
            var pivot = await _db.Chats.AsNoTracking().FirstOrDefaultAsync(c => c.Id == cursor.Value, ct);
            if (pivot != null)
            {
                query = query.Where(c => c.CreatedAtUtc > pivot.CreatedAtUtc || (c.CreatedAtUtc == pivot.CreatedAtUtc && c.Id > pivot.Id));
            }
        }

        var items = await query.Take(pageSize + 1).ToListAsync(ct);
        var hasMore = items.Count > pageSize;
        if (hasMore) items = items.Take(pageSize).ToList();

        var dtos = items.Select(c => new ChatDto(c.Id, c.Topic, c.CreatedAtUtc, c.Messages?.Count ?? 0));
        var next = hasMore ? items.Last().Id : (Guid?)null;
        return new PagedResponse<ChatDto>(dtos, next);
    }

    public async Task<ChatDto> CreateChatAsync(CreateChatRequest req, CancellationToken ct = default)
    {
        var chat = new Chat { Id = Guid.NewGuid(), Topic = req.Topic, CreatedAtUtc = DateTime.UtcNow };
        _db.Chats.Add(chat);
        await _db.SaveChangesAsync(ct);
        return new ChatDto(chat.Id, chat.Topic, chat.CreatedAtUtc, 0);
    }

    public async Task<ChatDto?> GetChatAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _db.Chats.AsNoTracking().Include(x => x.Messages).FirstOrDefaultAsync(x => x.Id == id, ct);
        if (c == null) return null;
        return new ChatDto(c.Id, c.Topic, c.CreatedAtUtc, c.Messages.Count);
    }

    public async Task DeleteChatAsync(Guid id, CancellationToken ct = default)
    {
        var c = await _db.Chats.FindAsync(new object[] { id }, ct);
        if (c == null) return;
        _db.Chats.Remove(c);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<PagedResponse<MessageDto>> GetMessagesAsync(Guid chatId, Guid? cursor, int pageSize, CancellationToken ct = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
    IQueryable<Models.Message> q = _db.Messages.AsNoTracking().Where(m => m.ChatId == chatId).OrderBy(m => m.CreatedAtUtc).ThenBy(m => m.Id);
        if (cursor is not null)
        {
            var pivot = await _db.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == cursor.Value, ct);
            if (pivot != null)
            {
                q = q.Where(m => m.CreatedAtUtc > pivot.CreatedAtUtc || (m.CreatedAtUtc == pivot.CreatedAtUtc && m.Id > pivot.Id));
            }
        }

        var items = await q.Take(pageSize + 1).ToListAsync(ct);
        var hasMore = items.Count > pageSize;
        if (hasMore) items = items.Take(pageSize).ToList();

        var dtos = items.Select(m => new MessageDto(m.Id, m.ChatId, m.Sender.ToString().ToLower(), m.Content, m.CreatedAtUtc));
        var next = hasMore ? items.Last().Id : (Guid?)null;
        return new PagedResponse<MessageDto>(dtos, next);
    }

    public async Task<MessageDto> CreateMessageAsync(Guid chatId, CreateMessageRequest req, CancellationToken ct = default)
    {
        var chat = await _db.Chats.FindAsync(new object[] { chatId }, ct);
        if (chat == null) throw new KeyNotFoundException("Chat not found");
        if (!Enum.TryParse<MessageSender>(req.Sender, true, out var sender)) sender = MessageSender.User;
        var m = new Message { Id = Guid.NewGuid(), ChatId = chatId, Content = req.Content, Sender = sender, CreatedAtUtc = DateTime.UtcNow };
        _db.Messages.Add(m);
        await _db.SaveChangesAsync(ct);
        return new MessageDto(m.Id, m.ChatId, m.Sender.ToString().ToLower(), m.Content, m.CreatedAtUtc);
    }

    public async Task<MessageDto?> GetMessageAsync(Guid messageId, CancellationToken ct = default)
    {
        var m = await _db.Messages.AsNoTracking().FirstOrDefaultAsync(x => x.Id == messageId, ct);
        if (m == null) return null;
        return new MessageDto(m.Id, m.ChatId, m.Sender.ToString().ToLower(), m.Content, m.CreatedAtUtc);
    }

    public async Task DeleteMessageAsync(Guid messageId, CancellationToken ct = default)
    {
        var m = await _db.Messages.FindAsync(new object[] { messageId }, ct);
        if (m == null) return;
        _db.Messages.Remove(m);
        await _db.SaveChangesAsync(ct);
    }
}
