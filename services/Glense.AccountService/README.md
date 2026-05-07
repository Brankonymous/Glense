# Account Service

Handles authentication, user profiles, and notifications for the Glense platform. Acts as the source of truth for user identities and serves a gRPC `AccountGrpc` endpoint that other services call to resolve usernames.

For the full REST request/response reference, see [ACCOUNT_API.md](ACCOUNT_API.md).

## Tech stack

- .NET 8.0, ASP.NET Core
- PostgreSQL 16 + Entity Framework Core
- JWT bearer auth, BCrypt password hashing
- gRPC server (HTTP/2 cleartext, secured by `INTERNAL_API_KEY`)
- MassTransit + RabbitMQ (consumer)

## Database schema

| Table | Columns |
|-------|---------|
| `users` | `id` (UUID PK), `username` (unique), `email` (unique), `password_hash`, `profile_picture_url?`, `account_type` (`user`/`creator`/`admin`), `created_at`, `updated_at?`, `is_active`, `is_verified` |
| `notifications` | `id` (UUID PK), `user_id` (FK → users), `title`, `message`, `type`, `is_read`, `related_entity_id?`, `created_at` |

EF Core context: [Data/AccountDbContext.cs](Data/AccountDbContext.cs).

## REST endpoints

All endpoints are reachable through the gateway under `http://localhost:5050`. Direct port (no gateway) is `http://localhost:5001`.

### Auth — [Controllers/AuthController.cs](Controllers/AuthController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/auth/register` | — | Register a new user, returns JWT |
| POST | `/api/auth/login` | — | Login by username or email, returns JWT |

### Profile — [Controllers/ProfileController.cs](Controllers/ProfileController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/profile/search?q=&limit=20` | — | Search users by username or email |
| GET | `/api/profile/me` | JWT | Current user's profile |
| GET | `/api/profile/{userId}` | — | Get user by id |
| PUT | `/api/profile/me` | JWT | Update current profile |
| DELETE | `/api/profile/me` | JWT | Soft-delete (deactivate) account |

### Notifications — [Controllers/NotificationController.cs](Controllers/NotificationController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/notification?isRead&skip=0&take=20` | JWT | List user's notifications |
| GET | `/api/notification/unread-count` | JWT | Unread counter |
| PUT | `/api/notification/{id}/read` | JWT | Mark single notification read |
| PUT | `/api/notification/read-all` | JWT | Mark all read |

### Internal — [Controllers/InternalController.cs](Controllers/InternalController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| POST | `/api/internal/notifications` | JWT | Create a notification (used by other services) |

### Health

- `GET /health` — service health probe.

## gRPC API

Server: `Glense.AccountService.Protos.AccountGrpc` — defined in [Protos/account.proto](Protos/account.proto).

| RPC | Purpose |
|-----|---------|
| `GetUsername(GetUsernameRequest)` | Resolve a single user id → username |
| `GetUsernames(GetUsernamesRequest)` | Batch resolve user ids → usernames |

All gRPC calls require the header `x-internal-api-key: <INTERNAL_API_KEY>`. Enforced by [Services/InternalApiKeyInterceptor.cs](Services/InternalApiKeyInterceptor.cs); calls without a valid key fail with `UNAUTHENTICATED`.

## Events (MassTransit / RabbitMQ)

### Published by Account Service

| Event | When | Payload |
|-------|------|---------|
| `UserRegisteredEvent` | After successful registration | `UserId`, `Username`, `Email` |

Consumed by Donation service to auto-create the user's wallet.

### Consumed by Account Service

| Event | Source | Effect |
|-------|--------|--------|
| `DonationMadeEvent` | Donation service | Creates a "you received a donation" notification |
| `UserSubscribedEvent` | Video service | Creates a "new subscriber" notification |

Event contracts live in [Messages/](Messages/).

## Configuration

| Variable | Default | Purpose |
|----------|---------|---------|
| `ACCOUNT_REST_PORT` | `5000` | REST listener port (in-container) |
| `ACCOUNT_GRPC_PORT` | `5001` | gRPC listener port (in-container) |
| `ConnectionStrings__DefaultConnection` | — | Npgsql connection string |
| `JWT_SECRET_KEY` | — | HMAC secret (≥32 chars) |
| `JWT_ISSUER` / `JWT_AUDIENCE` | `GlenseAccountService` / `GlenseApp` | JWT validation params |
| `INTERNAL_API_KEY` | — | Shared secret for service-to-service calls |
| `RabbitMQ__Host` / `RabbitMQ__Username` / `RabbitMQ__Password` | `localhost` / `guest` / `guest` | Broker connection |

## Running standalone

```bash
cd services/Glense.AccountService
dotnet run
```

Without a `ConnectionStrings__DefaultConnection`, the service falls back to an EF InMemory database (suitable for quick local testing). Swagger UI is at `http://localhost:5001/swagger`.

For full-stack setup, see [DEV_QUICKSTART.md](../../DEV_QUICKSTART.md).

## Inter-service dependencies

| Direction | Protocol | Purpose |
|-----------|----------|---------|
| Donation → Account | HTTP `GET /api/profile/{id}` | Validate recipient before transfer |
| Donation → Account | HTTP `POST /api/internal/notifications` | Donation notification fallback |
| Video → Account | gRPC `GetUsernames` | Resolve uploader names for video listings |
| Account ← Donation | RabbitMQ `DonationMadeEvent` | Notification trigger |
| Account ← Video | RabbitMQ `UserSubscribedEvent` | Notification trigger |
| Account → Donation | RabbitMQ `UserRegisteredEvent` | Trigger wallet creation |
