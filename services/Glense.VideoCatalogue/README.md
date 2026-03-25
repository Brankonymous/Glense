# Video Catalogue Microservice

Manages video metadata, comments, playlists, subscriptions, and likes for the Glense platform.

## API Endpoints

### Videos
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/videos` | List all videos (includes uploader username) |
| GET | `/api/videos/{id}` | Get video by ID |
| POST | `/api/videos/upload` | Upload video (multipart/form-data) |
| GET | `/api/videos/{id}/stream` | Stream video (supports range requests) |

### Comments
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/videos/{videoId}/comments` | Get comments for a video |
| POST | `/api/videos/{videoId}/comments` | Post a comment |
| DELETE | `/api/videos/{videoId}/comments/{commentId}` | Delete a comment |

### Subscriptions & Likes
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/subscriptions` | Subscribe to a creator |
| DELETE | `/api/subscriptions` | Unsubscribe |
| POST | `/api/videolikes` | Like/dislike a video |

### Playlists
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/playlists` | Create playlist |
| GET | `/api/playlists` | List playlists |
| GET | `/api/playlists/{id}` | Get playlist |
| POST | `/api/playlistvideos` | Add video to playlist |
| DELETE | `/api/playlistvideos` | Remove video from playlist |
| GET | `/api/playlistvideos/{playlistId}` | List videos in playlist |

### Health
- `GET /health` - Service health check

## Inter-Service Communication

The Video Catalogue service calls the Account service to resolve uploader usernames:

| Flow | Direction | Description |
|------|-----------|-------------|
| Video listing | Video → Account | Resolves uploader usernames via `GET /api/profile/{userId}` |

If the Account service is unavailable, videos are still returned — the `uploaderUsername` field will be `null`.

## Running Locally

```bash
cd services/Glense.VideoCatalogue
dotnet run
```

Or via Docker Compose:
```bash
docker compose up video_service postgres_video
```

## Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `ConnectionStrings__VideoCatalogue` | PostgreSQL connection string | In-memory DB |
| `ACCOUNT_SERVICE_URL` | Account service base URL | `http://localhost:5001` |
| `VideoStorage__BasePath` | Local file storage path | `Videos` |

## Seeding

Test videos and comments are seeded by `./scripts/seed.sh`. The script inserts videos with real user IDs as uploaders.
