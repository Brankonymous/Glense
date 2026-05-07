# Chat API

Base URL (direct): `http://localhost:5004`. Through gateway: `http://localhost:5050`.

Auth: all REST endpoints and the SignalR hub require JWT.

Source: [services/Glense.ChatService/Controllers/](../../services/Glense.ChatService/Controllers), [services/Glense.ChatService/Hubs/ChatHub.cs](../../services/Glense.ChatService/Hubs/ChatHub.cs).

## Chats ([ChatsController.cs](../../services/Glense.ChatService/Controllers/ChatsController.cs))

| Method | Path | Notes |
|--------|------|-------|
| `GET` | `/api/chats?cursor=<guid>&pageSize=50` | Cursor pagination |
| `POST` | `/api/chats` | Body `{ "topic": "..." }` (validated DTO) |
| `GET` | `/api/chats/{chatId:guid}` | `404` if not found |
| `DELETE` | `/api/chats/{chatId:guid}` | |

## Messages

### Per-chat collection ([MessagesController.cs](../../services/Glense.ChatService/Controllers/MessagesController.cs))

| Method | Path | Notes |
|--------|------|-------|
| `GET` | `/api/chats/{chatId:guid}/messages?cursor=<guid>&pageSize=50` | |
| `POST` | `/api/chats/{chatId:guid}/messages` | Body `{ "content": "..." }`. `userId`/`username` taken from JWT claims. `404` if chat doesn't exist. |

### Single-message addressing ([MessageRootController.cs](../../services/Glense.ChatService/Controllers/MessageRootController.cs))

| Method | Path | Notes |
|--------|------|-------|
| `GET` | `/api/messages/{messageId:guid}` | `404` if not found |
| `DELETE` | `/api/messages/{messageId:guid}` | |

## SignalR hub

Endpoint: `/hubs/chat` (proxied by gateway). Connection requires JWT (negotiation passes the bearer token).

| Direction | Member | Args |
|-----------|--------|------|
| Client → Server | `JoinChat` | `chatId: string` — adds connection to the chat group |
| Client → Server | `LeaveChat` | `chatId: string` |
| Client → Server | `SendMessageToChat` | `chatId, user, message` — broadcasts to group |
| Server → Client | `ReceiveMessage` | `chatId, user, message` |

> Persistence is performed by `POST /api/chats/{chatId}/messages`. The hub method above is currently a real-time broadcast only. Recommended client pattern: open a hub connection, then `POST` for persistence and broadcast via `SendMessageToChat` from the server-side handler if needed. See [flows/07-chat-realtime.md](../flows/07-chat-realtime.md).

## Health

`GET /health` — `{ status: "Healthy", service: "chat", timestamp }`.
