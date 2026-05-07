# Video Catalogue Service

Videos, comments, subscriptions, likes, and playlists.

## Tech stack

ASP.NET Core 8 · EF Core (Npgsql) · MassTransit/RabbitMQ · gRPC client · Swagger · 500 MB upload limit.

## Run

```bash
./dev.sh                     # full stack
# or
cd services/Glense.VideoCatalogue
dotnet run                   # http://localhost:5002
```

Swagger: http://localhost:5002/swagger.

## Endpoints (summary)

- Videos: list, search, get, upload (multipart, ≤500 MB), stream (HTTP range), thumbnail, view-increment, category update
- Comments: list/post/like/delete on `/api/videos/{id}/comments`
- Subscriptions: subscribe/unsubscribe (`/api/subscriptions`)
- Likes: get/post (`/api/videolikes`)
- Playlists: create/list/get + add/remove videos
- `GET /health`

Full reference: [../../docs/api/video-catalogue.md](../../docs/api/video-catalogue.md).

## Inter-service

| Direction | Mechanism | Purpose |
|-----------|-----------|---------|
| → Account | gRPC `AccountGrpc.GetUsername(s)` | Resolve uploader usernames |
| → RabbitMQ | publish `UserSubscribedEvent` | Notify channel owner via Account |

Outbound gRPC injects `INTERNAL_API_KEY` via [`InternalApiKeyClientInterceptor`](GrpcClients).

## Configuration

See [../../docs/CONFIGURATION.md](../../docs/CONFIGURATION.md). Key per-service vars: `ACCOUNT_GRPC_URL`, `VideoStorage:BasePath` (default `Videos/`).

## Storage

Files live on disk under `VideoStorage:BasePath`. In containers, mount a volume to persist uploads.

## Tests

[../../tests/VideoCatalogue.IntegrationTests/](../../tests/VideoCatalogue.IntegrationTests).
