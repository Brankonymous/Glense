# Chat Service

Real-time chat: REST CRUD for chats and messages plus a SignalR hub for live broadcast.

## Tech stack

ASP.NET Core 8 · EF Core (Npgsql, in-memory fallback) · SignalR · JWT · Swagger.

## Run

```bash
./dev.sh                     # full stack
# or
cd services/Glense.ChatService
dotnet run                   # http://localhost:5004
```

Swagger: http://localhost:5004/swagger.  
SignalR hub: `ws://localhost:5004/hubs/chat` (or via gateway at `/hubs/chat`).

## Endpoints (summary)

- Chats: `GET /api/chats`, `POST /api/chats`, `GET /api/chats/{id}`, `DELETE /api/chats/{id}`
- Messages: `GET|POST /api/chats/{chatId}/messages`, `GET|DELETE /api/messages/{id}`
- SignalR: `JoinChat`, `LeaveChat`, `SendMessageToChat` (client→server); `ReceiveMessage` (server→client)
- `GET /health`

Full reference: [../../docs/api/chat.md](../../docs/api/chat.md). Real-time flow: [../../docs/flows/07-chat-realtime.md](../../docs/flows/07-chat-realtime.md).

## Inter-service

None at runtime. JWTs are validated locally with the shared `JWT_SECRET_KEY`.

## Configuration

See [../../docs/CONFIGURATION.md](../../docs/CONFIGURATION.md). Key per-service vars: `CHAT_USE_INMEMORY` (force EF in-memory), `ConnectionStrings__DefaultConnection`, `JwtSettings__*`.

On first run the service seeds three demo chats (`General`, `Tech Talk`, `Gaming`) with placeholder messages — skipped under `ASPNETCORE_ENVIRONMENT=Testing`.

## Tests

[../../tests/ChatService.IntegrationTests/](../../tests/ChatService.IntegrationTests).
