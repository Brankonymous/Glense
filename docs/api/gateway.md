# API Gateway (YARP)

The gateway ([Glense.Server/](../../Glense.Server)) is a thin YARP reverse proxy. It does **not** perform authentication; it forwards requests verbatim and relies on each backend to validate JWTs. Public callers (browser, curl) should always go through the gateway at `http://localhost:5050`.

Configuration source: [Glense.Server/appsettings.json](../../Glense.Server/appsettings.json) → `ReverseProxy`.

## Route table

| Public path | Cluster | Backend service |
|-------------|---------|-----------------|
| `/api/auth/{**}`         | `account-cluster`  | http://localhost:5001 |
| `/api/profile/{**}`      | `account-cluster`  | http://localhost:5001 |
| `/api/notification/{**}` | `account-cluster`  | http://localhost:5001 |
| `/api/videos/{**}`       | `video-cluster`    | http://localhost:5002 |
| `/api/subscriptions/{**}`| `video-cluster`    | http://localhost:5002 |
| `/api/videolikes/{**}`   | `video-cluster`    | http://localhost:5002 |
| `/api/playlists/{**}`    | `video-cluster`    | http://localhost:5002 |
| `/api/playlistvideos/{**}` | `video-cluster`  | http://localhost:5002 |
| `/api/donation/{**}`     | `donation-cluster` | http://localhost:5100 |
| `/api/wallet/{**}`       | `donation-cluster` | http://localhost:5100 |
| `/api/chats/{**}`        | `chat-cluster`     | http://localhost:5004 |
| `/api/messages/{**}`     | `chat-cluster`     | http://localhost:5004 |
| `/hubs/chat/{**}`        | `chat-cluster`     | http://localhost:5004 (SignalR) |

## Built-in endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /health` | Gateway liveness probe |

## CORS

`Cors:AllowedOrigins` whitelist (defaults: `http://localhost:5173`, `:50653`, `:50654`, `:3000`). Credentials allowed.

## Cluster health checks

Active health probes hit `/health` on each backend every 10 s. Cluster-level activity timeouts:

| Cluster | Timeout |
|---------|---------|
| `account-cluster` | 30 s |
| `video-cluster` | 5 min (uploads) |
| `donation-cluster` | 30 s |
| `chat-cluster` | 30 s |
