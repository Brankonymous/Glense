# Account Service

Identity, profiles, and notifications for Glense.

## Tech stack

ASP.NET Core 8 · EF Core (Npgsql) · BCrypt · MassTransit/RabbitMQ · gRPC · Swagger.

## Run

```bash
# As part of the stack (recommended)
./dev.sh

# Standalone
cd services/Glense.AccountService
dotnet run            # listens on :5000 (REST) and :5001 (gRPC) by default
```

When run in containers, host ports are published as `5001` (REST) and `5003` (gRPC). See [docker-compose.yml](../../docker-compose.yml).

Swagger: http://localhost:5001/swagger.

## Endpoints (summary)

- `POST /api/auth/register`, `POST /api/auth/login`
- `GET /api/profile/search`, `GET /api/profile/me`, `GET /api/profile/{id}`, `PUT /api/profile/me`, `DELETE /api/profile/me`
- `GET /api/notification`, `GET /api/notification/unread-count`, `PUT /api/notification/{id}/read`, `PUT /api/notification/read-all`
- `POST /api/internal/notifications` (service-to-service fallback)
- gRPC `AccountGrpc.GetUsername` / `GetUsernames`
- `GET /health`

Full reference with payloads and error codes: [../../docs/api/account.md](../../docs/api/account.md).

## Inter-service

| Direction | Mechanism | Purpose |
|-----------|-----------|---------|
| ← Donation | RabbitMQ `DonationMadeEvent` | Create donation notification |
| ← Video | RabbitMQ `UserSubscribedEvent` | Create subscription notification |
| ← Donation | HTTP `GET /api/profile/{id}` | Sync recipient validation |
| ← Video | gRPC `AccountGrpc` | Username resolution (batch) |

Enforced by `INTERNAL_API_KEY` ([`InternalApiKeyInterceptor`](GrpcServices)).

## Configuration

See [../../docs/CONFIGURATION.md](../../docs/CONFIGURATION.md). Key per-service vars: `ACCOUNT_REST_PORT` (default `5000`), `ACCOUNT_GRPC_PORT` (default `5001`).

## Database

Schema (DDL): [database/schema.sql](database/schema.sql). Tables: `users`, `notifications`. Auto-loaded by the Postgres container init script when using `dev.sh`.

## Tests

[../../tests/AccountService.IntegrationTests/](../../tests/AccountService.IntegrationTests). Run with `dotnet test tests/AccountService.IntegrationTests`.
