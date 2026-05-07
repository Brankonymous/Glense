# Donation Service

Manages user wallets and donations between users in the Glense platform. Lives under `Glense.Server/DonationService/` (alongside the API gateway), but is otherwise an independent microservice with its own database.

## Tech stack

- .NET 8.0, ASP.NET Core
- PostgreSQL 16 + Entity Framework Core (falls back to InMemory when no connection string)
- JWT bearer auth (tokens issued by Account)
- HTTP client to Account service (recipient validation)
- MassTransit + RabbitMQ (publisher and consumer)

## Database schema

EF Core context: [Data/DonationDbContext.cs](Data/DonationDbContext.cs).

| Table | Columns |
|-------|---------|
| `Wallets` | `Id` (UUID PK), `UserId` (unique), `Balance` (decimal), `CreatedAt`, `UpdatedAt` |
| `Donations` | `Id` (UUID PK), `DonorUserId`, `RecipientUserId`, `Amount` (decimal), `Message?`, `CreatedAt` |

## REST endpoints

Reachable through the gateway. Direct port: `http://localhost:5100`.

### Donations — [Controllers/DonationController.cs](Controllers/DonationController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/donation/donor/{userId}` | JWT | Donations sent by a user |
| GET | `/api/donation/recipient/{userId}` | JWT | Donations received by a user |
| POST | `/api/donation` | JWT | Create a donation: validates recipient, debits donor wallet, credits recipient wallet, publishes `DonationMadeEvent` |

### Wallets — [Controllers/WalletController.cs](Controllers/WalletController.cs)

| Method | Path | Auth | Purpose |
|--------|------|------|---------|
| GET | `/api/wallet/user/{userId}` | JWT | Get a user's wallet |
| POST | `/api/wallet` | JWT | Create a wallet (idempotent — returns existing if any) |
| POST | `/api/wallet/user/{userId}/topup` | JWT | Add funds to a wallet |

### Health & docs

- `GET /health` — service health probe
- `GET /swagger` — Swagger UI

## Events (MassTransit / RabbitMQ)

### Consumed

| Event | Source | Effect |
|-------|--------|--------|
| `UserRegisteredEvent` | Account service | Auto-creates a wallet for the new user |

### Published

| Event | When | Payload | Consumer |
|-------|------|---------|----------|
| `DonationMadeEvent` | After a successful donation transfer | `DonorId`, `RecipientId`, `Amount`, `DonorUsername` | Account service (creates "you received a donation" notification) |

## Outbound HTTP

The donation flow validates the recipient by calling Account directly:

| Call | Endpoint | Header |
|------|----------|--------|
| Recipient lookup | `GET /api/profile/{userId}` (Account) | `X-Internal-Api-Key: <INTERNAL_API_KEY>` |

Implemented in [Services/AccountServiceClient.cs](Services/AccountServiceClient.cs).

## Configuration

| Variable | Default | Purpose |
|----------|---------|---------|
| `ConnectionStrings__DonationDb` / `DONATION_DB_CONNECTION_STRING` | InMemory DB | Npgsql connection string |
| `JWT_SECRET_KEY` / `JwtSettings:SecretKey` | — | JWT signing secret |
| `JwtSettings:Issuer` / `JwtSettings:Audience` | `GlenseAccountService` / `GlenseApp` | JWT validation |
| `INTERNAL_API_KEY` | — | Sent on outbound HTTP calls to Account |
| `AccountService:BaseUrl` | `http://localhost:5001` | Base URL for the Account REST client |
| `RabbitMQ__Host` / `RabbitMQ__Username` / `RabbitMQ__Password` | `localhost` / `guest` / `guest` | Broker |

## Running standalone

```bash
cd Glense.Server/DonationService
dotnet run
```

Swagger UI is at `http://localhost:5100`. For full-stack setup, see [DEV_QUICKSTART.md](../../DEV_QUICKSTART.md).
