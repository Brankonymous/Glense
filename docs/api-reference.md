# API Reference

Index of every public API across the platform. All endpoints are reachable through the gateway at `http://localhost:5050`. The gateway routes by URL prefix to the owning service.

For request/response schemas of the Account service see [../services/Glense.AccountService/ACCOUNT_API.md](../services/Glense.AccountService/ACCOUNT_API.md). For other services, see the per-service READMEs linked below.

## Gateway routes

Defined in [../Glense.Server/appsettings.json](../Glense.Server/appsettings.json) under `ReverseProxy:Routes`.

| Prefix | Routed to |
|--------|-----------|
| `/api/auth/*` | Account |
| `/api/profile/*` | Account |
| `/api/notification/*` | Account |
| `/api/videos/*` | Video Catalogue |
| `/api/subscriptions/*` | Video Catalogue |
| `/api/videolikes/*` | Video Catalogue |
| `/api/playlists/*` | Video Catalogue |
| `/api/playlistvideos/*` | Video Catalogue |
| `/api/donation/*` | Donation |
| `/api/wallet/*` | Donation |
| `/api/chats/*` | Chat |
| `/api/messages/*` | Chat |
| `/hubs/chat` | Chat (SignalR / WebSocket) |

## Account — [README](../services/Glense.AccountService/README.md) · [ACCOUNT_API.md](../services/Glense.AccountService/ACCOUNT_API.md)

| Method | Path | Auth |
|--------|------|------|
| POST | `/api/auth/register` | — |
| POST | `/api/auth/login` | — |
| GET | `/api/profile/search?q=&limit=` | — |
| GET | `/api/profile/me` | JWT |
| GET | `/api/profile/{userId}` | — |
| PUT | `/api/profile/me` | JWT |
| DELETE | `/api/profile/me` | JWT |
| GET | `/api/notification` | JWT |
| GET | `/api/notification/unread-count` | JWT |
| PUT | `/api/notification/{id}/read` | JWT |
| PUT | `/api/notification/read-all` | JWT |
| POST | `/api/internal/notifications` | JWT (internal use) |

gRPC: `AccountGrpc.GetUsername`, `AccountGrpc.GetUsernames` — see [../services/Glense.AccountService/Protos/account.proto](../services/Glense.AccountService/Protos/account.proto).

## Video Catalogue — [README](../services/Glense.VideoCatalogue/README.md)

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/videos` | — |
| GET | `/api/videos/{id}` | — |
| GET | `/api/videos/search?q=&category=` | — |
| POST | `/api/videos/upload` | JWT |
| GET | `/api/videos/{id}/stream` | — |
| GET | `/api/videos/{id}/thumbnail` | — |
| POST | `/api/videos/{id}/view` | — |
| GET | `/api/videos/{videoId}/comments` | — |
| POST | `/api/videos/{videoId}/comments` | JWT |
| DELETE | `/api/videos/{videoId}/comments/{commentId}` | JWT |
| POST | `/api/subscriptions` | JWT |
| DELETE | `/api/subscriptions` | JWT |
| POST | `/api/videolikes` | JWT |
| POST | `/api/playlists` | JWT |
| GET | `/api/playlists` | — |
| GET | `/api/playlists/{id}` | — |
| POST | `/api/playlistvideos` | JWT |
| DELETE | `/api/playlistvideos` | JWT |
| GET | `/api/playlistvideos/{playlistId}` | — |

## Donation — [README](../Glense.Server/DonationService/README.md)

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/donation/donor/{userId}` | JWT |
| GET | `/api/donation/recipient/{userId}` | JWT |
| POST | `/api/donation` | JWT |
| GET | `/api/wallet/user/{userId}` | JWT |
| POST | `/api/wallet` | JWT |
| POST | `/api/wallet/user/{userId}/topup` | JWT |

## Chat — [README](../services/Glense.ChatService/README.md)

| Method | Path | Auth |
|--------|------|------|
| GET | `/api/chats?cursor=&pageSize=` | JWT |
| POST | `/api/chats` | JWT |
| GET | `/api/chats/{chatId}` | JWT |
| DELETE | `/api/chats/{chatId}` | JWT |
| GET | `/api/chats/{chatId}/messages?cursor=&pageSize=` | JWT |
| POST | `/api/chats/{chatId}/messages` | JWT |
| GET | `/api/messages/{messageId}` | JWT |
| DELETE | `/api/messages/{messageId}` | JWT |

SignalR hub: `/hubs/chat` — methods `JoinChat`, `LeaveChat`, `SendMessageToChat`; client event `ReceiveMessage`.

## Health endpoints

Every service exposes `GET /health` returning `{ status: "healthy", service, timestamp }`. The gateway also exposes its own `/health`.

## Live Swagger UI

When running, each service serves Swagger:

| Service | URL |
|---------|-----|
| Account | http://localhost:5001/swagger |
| Video Catalogue | http://localhost:5002/swagger |
| Donation | http://localhost:5100/swagger |
| Chat | http://localhost:5004/swagger |
