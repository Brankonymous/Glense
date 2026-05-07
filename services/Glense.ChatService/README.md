# Chat Service

Real-time chat for the Glense platform. Combines REST endpoints for chat/message management with a SignalR hub for live message delivery.

## Tech stack

- .NET 8.0, ASP.NET Core
- PostgreSQL 16 + Entity Framework Core (falls back to InMemory when no connection string)
- JWT bearer auth (tokens issued by Account)
- SignalR (`/hubs/chat`)

## Database schema

EF Core context: [Data/ChatDbContext.cs](Data/ChatDbContext.cs).

| Table | Columns |
|-------|---------|
| `Chats` | `Id` (UUID PK), `Title`, `CreatedAt`, … (see entity) |
| `Messages` | `Id` (UUID PK), `ChatId` (FK → Chats), `UserId`, `Username`, `Body`, `CreatedAt` |

## REST endpoints

Reachable through the gateway. Direct port: `http://localhost:5004`.

### Chats — [Controllers/ChatsController.cs](Controllers/ChatsController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/chats?cursor=&pageSize=50` | JWT | Cursor-paginated list of chats |
| POST | `/api/chats` | JWT | Create a chat room |
| GET | `/api/chats/{chatId}` | JWT | Get a chat |
| DELETE | `/api/chats/{chatId}` | JWT | Delete a chat |

### Messages — [Controllers/MessagesController.cs](Controllers/MessagesController.cs), [Controllers/MessageRootController.cs](Controllers/MessageRootController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/chats/{chatId}/messages?cursor=&pageSize=50` | JWT | List messages |
| POST | `/api/chats/{chatId}/messages` | JWT | Send a message (user id / username extracted from JWT) |
| GET | `/api/messages/{messageId}` | JWT | Get a single message |
| DELETE | `/api/messages/{messageId}` | JWT | Delete a message |

### Health

- `GET /health` — service health probe.

## SignalR hub

Hub URL: `/hubs/chat` (proxied by the gateway from `http://localhost:5050/hubs/chat`).

Server methods clients invoke ([Hubs/ChatHub.cs](Hubs/ChatHub.cs)):

| Method | Args | Effect |
|--------|------|--------|
| `JoinChat` | `chatId: string` | Add the connection to the chat group |
| `LeaveChat` | `chatId: string` | Remove from the group |
| `SendMessageToChat` | `chatId, user, message` | Broadcast `ReceiveMessage` to the group |

Client-bound events:

| Event | Args | Meaning |
|-------|------|---------|
| `ReceiveMessage` | `chatId, user, message` | A new message has been broadcast |

> Note: persisting a message to the database happens via the REST `POST /api/chats/{chatId}/messages` endpoint. The hub itself only broadcasts; the typical client flow is "POST to persist, then trigger `SendMessageToChat` to fan-out".

## Configuration

| Variable | Default | Purpose |
|----------|---------|---------|
| `ConnectionStrings__DefaultConnection` | InMemory DB | Npgsql connection string |
| `JWT_SECRET_KEY` / `JwtSettings:SecretKey` | — | JWT signing secret |
| `JwtSettings:Issuer` / `JwtSettings:Audience` | `GlenseAccountService` / `GlenseApp` | JWT validation |

## Running standalone

```bash
cd services/Glense.ChatService
dotnet run
```

Swagger UI is at `http://localhost:5004/swagger`. For full-stack setup, see [DEV_QUICKSTART.md](../../DEV_QUICKSTART.md).
