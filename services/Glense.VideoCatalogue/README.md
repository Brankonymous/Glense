# Video Catalogue Service

Owns video metadata, file storage, comments, playlists, subscriptions, likes, and view counts.

## Tech stack

- .NET 8.0, ASP.NET Core
- PostgreSQL 16 + Entity Framework Core
- JWT bearer auth (validates tokens issued by Account)
- gRPC client (calls Account service for username lookups)
- MassTransit + RabbitMQ (publisher)
- Local file storage for uploaded videos and thumbnails

## Database schema

EF Core context: [Data/VideoCatalogueDbContext.cs](Data/VideoCatalogueDbContext.cs).

| Table | Highlights |
|-------|-----------|
| `Videos` | Id, Title, Description, VideoUrl, ThumbnailUrl, UploadDate, UploaderId, ViewCount, LikeCount, DislikeCount, Category |
| `Comments` | Id, VideoId (FK), UserId, Body, CreatedAt |
| `Subscriptions` | SubscriberId, SubscribedToId, SubscriptionDate (composite key) |
| `VideoLikes` | UserId, VideoId, IsLike (true=like, false=dislike) |
| `Playlists` | Id, OwnerId, Title, Description, CreatedAt |
| `PlaylistVideos` | PlaylistId, VideoId, AddedAt |

## REST endpoints

Reachable through the gateway under `http://localhost:5050`. Direct port: `http://localhost:5002`.

### Videos — [Controllers/VideosController.cs](Controllers/VideosController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/videos` | — | List videos (with resolved uploader usernames) |
| GET | `/api/videos/{id}` | — | Get a video by id |
| GET | `/api/videos/search?q=&category=` | — | Search by title/description (optionally by category) |
| POST | `/api/videos/upload` | JWT | Upload `multipart/form-data`: `File`, `Title`, `Description`, `Thumbnail`, `Category` |
| GET | `/api/videos/{id}/stream` | — | Stream video bytes (HTTP Range supported) |
| GET | `/api/videos/{id}/thumbnail` | — | Serve uploaded thumbnail |
| POST | `/api/videos/{id}/view` | — | Increment view count (rate-limited per IP via memory cache) |

### Comments — [Controllers/CommentsController.cs](Controllers/CommentsController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/videos/{videoId}/comments` | — | List comments |
| POST | `/api/videos/{videoId}/comments` | JWT | Post a comment |
| DELETE | `/api/videos/{videoId}/comments/{commentId}` | JWT | Delete own comment |

### Subscriptions — [Controllers/SubscriptionsController.cs](Controllers/SubscriptionsController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/subscriptions` | JWT | Subscribe to a creator (publishes `UserSubscribedEvent`) |
| DELETE | `/api/subscriptions` | JWT | Unsubscribe |

### Likes — [Controllers/VideoLikesController.cs](Controllers/VideoLikesController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/videolikes` | JWT | Like or dislike a video |

### Playlists — [Controllers/PlaylistsController.cs](Controllers/PlaylistsController.cs), [Controllers/PlaylistVideosController.cs](Controllers/PlaylistVideosController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/playlists` | JWT | Create playlist |
| GET | `/api/playlists` | — | List playlists |
| GET | `/api/playlists/{id}` | — | Get playlist |
| POST | `/api/playlistvideos` | JWT | Add video to playlist |
| DELETE | `/api/playlistvideos` | JWT | Remove video from playlist |
| GET | `/api/playlistvideos/{playlistId}` | — | List videos in a playlist |

### Health

- `GET /health` — service health probe.

## gRPC client

Calls Account service via [GrpcClients/AccountGrpcClient.cs](GrpcClients/AccountGrpcClient.cs). Every request adds the `x-internal-api-key` header through [GrpcClients/InternalApiKeyClientInterceptor.cs](GrpcClients/InternalApiKeyClientInterceptor.cs).

| Call | Used for |
|------|----------|
| `GetUsername(userId)` | Resolve uploader on a single-video page |
| `GetUsernames(userIds)` | Batch-resolve uploader usernames on listings |

If Account is unreachable, listings still return — `uploaderUsername` simply comes back null.

## Events (MassTransit / RabbitMQ)

| Event | When | Payload | Consumer |
|-------|------|---------|----------|
| `UserSubscribedEvent` | On successful `POST /api/subscriptions` | `SubscriberId`, `ChannelOwnerId`, `SubscriberUsername` | Account service (creates notification) |

## Configuration

| Variable | Default | Purpose |
|----------|---------|---------|
| `ConnectionStrings__VideoCatalogue` | InMemory DB | Npgsql connection string |
| `JWT_SECRET_KEY` / `JwtSettings:SecretKey` | — | JWT signing secret |
| `JwtSettings:Issuer` / `JwtSettings:Audience` | `GlenseAccountService` / `GlenseApp` | JWT validation |
| `INTERNAL_API_KEY` | — | Header on outbound gRPC calls to Account |
| `AccountService:GrpcUrl` | `http://localhost:5003` | Account gRPC endpoint |
| `RabbitMQ__Host` / `RabbitMQ__Username` / `RabbitMQ__Password` | `localhost` / `guest` / `guest` | Broker |
| `VideoStorage:Path` | `wwwroot/uploads` | Where uploaded files are written |

## Running standalone

```bash
cd services/Glense.VideoCatalogue
dotnet run
```

Swagger UI is at `http://localhost:5002/swagger`. For full-stack setup, see [DEV_QUICKSTART.md](../../DEV_QUICKSTART.md).
