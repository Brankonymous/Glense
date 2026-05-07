# Configuration

Every environment variable consumed by Glense, where it's read, and its default. Source-of-truth files: [.env.example](../.env.example), [docker-compose.yml](../docker-compose.yml), each service's `Program.cs` and `appsettings.json`.

## Resolution order

For most settings, services check **environment variable → `appsettings.json` → hard-coded default** (in that order). Connection strings can be supplied either as a top-level env var (e.g. `ACCOUNT_DB_CONNECTION_STRING`) or via the standard `ConnectionStrings__<Name>` double-underscore form.

## Shared secrets

| Variable | Used by | Default | Purpose |
|----------|---------|---------|---------|
| `JWT_SECRET_KEY` | All services | — (required) | HS256 signing key (≥32 chars) |
| `JWT_ISSUER` | All services | `GlenseAccountService` | JWT `iss` claim |
| `JWT_AUDIENCE` | All services | `GlenseApp` | JWT `aud` claim |
| `INTERNAL_API_KEY` | Account, Video, Donation | — | Service-to-service auth header |

## Database connection strings

| Variable | Service | Equivalent appsettings key | Fallback |
|----------|---------|----------------------------|----------|
| `ACCOUNT_DB_CONNECTION_STRING` | Account | `ConnectionStrings:DefaultConnection` | EF in-memory |
| `VIDEO_DB_CONNECTION_STRING` | Video | `ConnectionStrings:VideoCatalogue` | EF in-memory |
| `DONATION_DB_CONNECTION_STRING` | Donation | `ConnectionStrings:DonationDb` | EF in-memory |
| `CHAT_DB_CONNECTION_STRING` | Chat | `ConnectionStrings:DefaultConnection` | EF in-memory |
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | Postgres containers | — | `glense` / `changeme` |

## RabbitMQ

| Variable | Used by | Default |
|----------|---------|---------|
| `RABBITMQ_USER` (mapped to `RabbitMQ__Username`) | Account, Video, Donation | `guest` |
| `RABBITMQ_PASS` (mapped to `RabbitMQ__Password`) | Account, Video, Donation | `guest` |
| `RabbitMQ__Host` | Account, Video, Donation | `localhost` (host) / `rabbitmq` (compose) |

## Per-service variables

### Account ([Program.cs](../services/Glense.AccountService/Program.cs))

| Variable | Default | Purpose |
|----------|---------|---------|
| `ACCOUNT_REST_PORT` | `5000` | Kestrel HTTP/1.1 port (REST) |
| `ACCOUNT_GRPC_PORT` | `5001` | Kestrel HTTP/2 port (gRPC) |
| `Cors:AllowedOrigins` (array) | `localhost:5173/50653/50654/3000` | CORS allowlist |

### Video Catalogue ([Program.cs](../services/Glense.VideoCatalogue/Program.cs))

| Variable | Default | Purpose |
|----------|---------|---------|
| `ACCOUNT_GRPC_URL` / `AccountService:GrpcUrl` | `http://localhost:5001` | gRPC endpoint of Account |
| `ACCOUNT_SERVICE_URL` | `http://localhost:5001` | (compose only) Account REST URL |
| `VideoStorage:BasePath` | `Videos` (relative) | Local directory where uploads are stored |
| `Cors:AllowedOrigins` | as above | CORS allowlist |

### Donation ([Program.cs](../Glense.Server/DonationService/Program.cs))

| Variable | Default | Purpose |
|----------|---------|---------|
| `PORT` | `5100` | Listen port |
| `ACCOUNT_SERVICE_URL` | `http://localhost:5001` | Account REST base URL (recipient validation, profile lookups) |
| `JwtSettings__SecretKey` / `JWT_SECRET_KEY` | — | JWT signing key |
| `Cors:AllowedOrigins` | as above | CORS allowlist |

### Chat ([Program.cs](../services/Glense.ChatService/Program.cs))

| Variable | Default | Purpose |
|----------|---------|---------|
| `CHAT_URLS` / `ASPNETCORE_URLS` | (Kestrel default) | Override listen URL(s) |
| `CHAT_USE_INMEMORY` | `false` | Force EF in-memory (otherwise determined by connection string) |
| `JwtSettings__*` | as above | JWT validation |

### Gateway ([Glense.Server/Program.cs](../Glense.Server/Program.cs))

| Variable | Default | Purpose |
|----------|---------|---------|
| `ASPNETCORE_URLS` | `http://+:5050` (Docker profile) | Listen URL |
| `ReverseProxy:*` (appsettings) | See [appsettings.json](../Glense.Server/appsettings.json) | YARP routes & clusters |
| `Cors:AllowedOrigins` | as above | CORS allowlist |

### Frontend ([glense.client/](../glense.client))

| Variable | Default | Purpose |
|----------|---------|---------|
| `VITE_API_URL` | `http://localhost:5050` | Gateway base URL |
| `VITE_ACCOUNT_API_URL` | (legacy) | Direct Account URL — only when bypassing the gateway |

See [glense.client/README.md](../glense.client/README.md) for the full frontend env reference.
