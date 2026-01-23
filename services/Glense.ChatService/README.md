# Glense.ChatService

Minimal chat microservice used by the Glense solution.

Quickstart (local):

1. Update `appsettings.json` or set `DefaultConnection` env var with a Postgres connection.
2. Run migrations (or allow EF to create DB):

   dotnet ef migrations add InitialCreate -p . -s .
   dotnet ef database update -p . -s .

3. Run locally:

   dotnet run --project .

Docker:

  docker build -t glense_chat_service .
  docker run --rm -p 5004:5000 -e ASPNETCORE_ENVIRONMENT=Development glense_chat_service

Endpoints:

- GET /health
- GET /api/chats?cursor=<uuid>&pageSize=50
- POST /api/chats
- GET /api/chats/{chatId}
- DELETE /api/chats/{chatId}
- GET /api/chats/{chatId}/messages?cursor=<uuid>&pageSize=50
- POST /api/chats/{chatId}/messages
- GET /api/messages/{messageId}
- DELETE /api/messages/{messageId}
