# Chat Microservice

Real-time chat service for the Glense platform with SignalR support.

## API Endpoints

### Chats
- `GET /api/chats?cursor=<uuid>&pageSize=50` - List chats (cursor pagination)
- `POST /api/chats` - Create a chat room
- `GET /api/chats/{chatId}` - Get chat by ID
- `DELETE /api/chats/{chatId}` - Delete chat

### Messages
- `GET /api/chats/{chatId}/messages?cursor=<uuid>&pageSize=50` - List messages
- `POST /api/chats/{chatId}/messages` - Send a message (userId + username from JWT)
- `GET /api/messages/{messageId}` - Get message by ID
- `DELETE /api/messages/{messageId}` - Delete message

### Health
- `GET /health` - Service health check

### WebSocket
- `/hubs/chat` - SignalR hub for real-time messaging

## Running Locally

```bash
cd services/Glense.ChatService
dotnet run
```

Or via Docker Compose from the repo root:
```bash
docker compose up chat_service postgres_chat
```

## Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | In-memory DB |
| `JwtSettings__SecretKey` | JWT secret (must match Account service) | — |
| `JwtSettings__Issuer` | JWT issuer | `GlenseAccountService` |
| `JwtSettings__Audience` | JWT audience | `GlenseApp` |
