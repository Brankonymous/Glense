# Video Catalogue API

Base URL (direct): `http://localhost:5002`. Through gateway: `http://localhost:5050`.

Auth: search/list/get/stream/thumbnail/view are public; mutating endpoints require JWT.

Source: [services/Glense.VideoCatalogue/Controllers/](../../services/Glense.VideoCatalogue/Controllers).

## Videos ([VideosController.cs](../../services/Glense.VideoCatalogue/Controllers/VideosController.cs))

| Method | Path | Auth | Notes |
|--------|------|------|-------|
| `GET` | `/api/videos` | — | List all videos with resolved uploader usernames (gRPC → Account) |
| `GET` | `/api/videos/search?q=&category=` | — | Case-insensitive `ILIKE` over title + description; ordered by `viewCount` desc |
| `GET` | `/api/videos/{id:guid}` | — | Single video |
| `POST` | `/api/videos/upload` | JWT | `multipart/form-data` with `File`, `Title`, `Description`, `Thumbnail?`, `Category?`. 500 MB limit. |
| `GET` | `/api/videos/{id:guid}/stream` | — | Streams the file with HTTP range support |
| `GET` | `/api/videos/{id:guid}/thumbnail` | — | Returns the thumbnail file if any |
| `PATCH` | `/api/videos/{id:guid}/view` | — | Increments view count; per-IP de-dup for 30 min |
| `PATCH` | `/api/videos/{id:guid}/category` | JWT, owner only | Body: `{ "category": "..." }` |

Response shape (`UploadResponseDTO`):
```json
{ "id": "<guid>", "title": "...", "description": "...", "videoUrl": "...", "thumbnailUrl": "/api/videos/<id>/thumbnail",
  "uploadDate": "...", "uploaderId": "<guid>", "uploaderUsername": "keki",
  "viewCount": 0, "likeCount": 0, "dislikeCount": 0, "category": "Music" }
```

## Comments ([CommentsController.cs](../../services/Glense.VideoCatalogue/Controllers/CommentsController.cs))

| Method | Path | Auth |
|--------|------|------|
| `GET` | `/api/videos/{videoId:guid}/comments` | — |
| `POST` | `/api/videos/{videoId:guid}/comments` | JWT — body `{ "content": "..." }` |
| `POST` | `/api/videos/{videoId:guid}/comments/{commentId:guid}/like` | JWT — body `{ "isLiked": true }` |
| `DELETE` | `/api/videos/{videoId:guid}/comments/{commentId:guid}` | JWT, author only |

## Subscriptions ([SubscriptionsController.cs](../../services/Glense.VideoCatalogue/Controllers/SubscriptionsController.cs))

| Method | Path | Auth | Notes |
|--------|------|------|-------|
| `POST` | `/api/subscriptions` | JWT | Body `{ "subscribedToId": "<guid>" }`. `409` if already subscribed. Publishes `UserSubscribedEvent`. |
| `DELETE` | `/api/subscriptions` | JWT | Body `{ "subscribedToId": "<guid>" }` |

## Likes ([VideoLikesController.cs](../../services/Glense.VideoCatalogue/Controllers/VideoLikesController.cs))

| Method | Path | Auth | Notes |
|--------|------|------|-------|
| `GET` | `/api/videolikes/{videoId:guid}` | JWT | Returns `{ "liked": true|false|null }` |
| `POST` | `/api/videolikes` | JWT | Body `{ "videoId": "<guid>", "isLiked": true }`. Toggle/switch handled server-side. |

## Playlists ([PlaylistsController.cs](../../services/Glense.VideoCatalogue/Controllers/PlaylistsController.cs))

| Method | Path | Auth | Notes |
|--------|------|------|-------|
| `POST` | `/api/playlists` | JWT | Body `{ "name", "description" }` |
| `GET` | `/api/playlists` | — | Optional header `X-Creator-Id: <guid>` to filter |
| `GET` | `/api/playlists/{id:guid}` | — | |

## Playlist videos ([PlaylistVideosController.cs](../../services/Glense.VideoCatalogue/Controllers/PlaylistVideosController.cs))

| Method | Path | Auth | Notes |
|--------|------|------|-------|
| `POST` | `/api/playlistvideos` | JWT, playlist owner only | Body `{ "playlistId", "videoId" }`. `409` if video already in playlist. |
| `DELETE` | `/api/playlistvideos` | JWT, playlist owner only | Same body |
| `GET` | `/api/playlistvideos/{playlistId:guid}` | — | List videos in playlist |

## Health

`GET /health` — implemented via `MapHealthChecks("/health")`.

## Inter-service

- gRPC client → Account `AccountGrpc.GetUsernames` (batch) on every list/search and `GetUsername` on detail.
- RabbitMQ producer → `UserSubscribedEvent` on subscribe (consumed by Account).
- All gRPC outbound calls inject `INTERNAL_API_KEY` via [`InternalApiKeyClientInterceptor`](../../services/Glense.VideoCatalogue/GrpcClients).

## Storage

Files are stored on local disk under `VideoStorage:BasePath` (default `Videos/` next to the service). Use a Docker volume in production.
