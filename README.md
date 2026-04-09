# Glense

A microservice-based video streaming platform built with .NET 8, React, and PostgreSQL.

## Architecture

```
                        ┌─────────────────┐
                        │    Frontend      │
                        │   (React/Vite)   │
                        └────────┬─────────┘
                                 │
                      ┌──────────▼──────────────┐
                      │      API Gateway        │
                      │      YARP :5050         │
                      │                         │
                      │ /api/auth/*    → Account │
                      │ /api/profile/* → Account │
                      │ /api/videos/*  → Video   │
                      │ /api/donation/*→ Donation│
                      │ /api/chats/*   → Chat    │
                      │ /hubs/chat     → Chat(WS)│
                      └──┬───┬───┬───┬──────────┘
           ┌─────────────┘   │   │   └──────────┐
           ▼                 ▼   ▼              ▼
    ┌─────────────┐  ┌────────────┐  ┌─────────────┐  ┌─────────────┐
    │   Account   │  │  Donation  │  │    Video    │  │    Chat     │
    │    :5001    │  │   :5100    │  │  Catalogue  │  │    :5004    │
    │             │  │            │  │    :5002    │  │             │
    │ Auth        │  │ Wallets    │  │ Upload      │  │ Rooms       │
    │ Profiles    │  │ Donations  │  │ Comments    │  │ Messages    │
    │ Notifs      │  │            │  │ Playlists   │  │ SignalR     │
    │ gRPC server │  │            │  │ gRPC client │  │             │
    └──────┬──────┘  └──────┬─────┘  └──────┬──────┘  └─────────────┘
           │                │               │
           │         ┌──────┴──────┐        │
           │         │  RabbitMQ   │        │
           │         │ :5672/:15672│        │
           │         └─────────────┘        │
           │                                │
           │◄─── RabbitMQ ──── Donation        wallet create on registration
           │◄─── HTTP ──────── Donation        validate recipient
           │◄─── RabbitMQ ──── Donation        donation notification
           │◄─── gRPC ──────────────── Video   resolve usernames
           │◄─── RabbitMQ ─────────── Video    subscription notification
```

### Services

| Service | Port | Database | What it does |
|---------|------|----------|--------------|
| API Gateway (YARP) | 5050 | -- | Routes all requests, CORS whitelist, health checks |
| Account | 5001 (REST), 5003 (gRPC) | PostgreSQL :5432 | Auth, profiles, notifications, user search, gRPC username server |
| Video Catalogue | 5002 | PostgreSQL :5433 | Upload, search, comments, playlists, subscriptions |
| Donation | 5100 | PostgreSQL :5434 | Wallets, donations, balance transfers |
| Chat | 5004 | PostgreSQL :5435 | Chat rooms, messages, real-time via SignalR |
| RabbitMQ | 5672 / 15672 | -- | Async event broker (MassTransit) |

### Inter-service communication

| Flow | Protocol | Why |
|------|----------|-----|
| Wallet creation on registration | Account → Donation via **RabbitMQ** | Fire-and-forget |
| Donation notifications | Donation → Account via **RabbitMQ** | Async, doesn't block payment |
| Subscription notifications | Video → Account via **RabbitMQ** | Async |
| Recipient validation | Donation → Account via **HTTP** | Sync check before transfer |
| Username resolution | Video → Account via **gRPC** | High-perf batch lookups (Protobuf) |

All inter-service calls (HTTP and gRPC) are authenticated with a shared API key (`INTERNAL_API_KEY`).

## Quick start

### Prerequisites

- Docker or Podman
- Node.js v22 (for frontend)

### 1. Configure environment

```bash
cp .env.example .env
# Edit .env with your secrets (defaults work for local dev)
```

### 2. Start everything

```bash
# Using the dev script:
./dev.sh

# Or manually:
docker compose up --build -d
```

### 3. Start the frontend

```bash
cd glense.client && npm install && npm run dev
```

### Verify it works

```bash
# Health checks
curl http://localhost:5050/health          # Gateway
curl http://localhost:5001/health          # Account
curl http://localhost:5002/health          # Video
curl http://localhost:5100/health          # Donation
curl http://localhost:5004/health          # Chat

# Register + login
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Password123!"}'

curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"test","password":"Password123!"}'
```

### Seed test data

```bash
./scripts/seed.sh
```

Creates 3 users (password: `Password123!`): `keki`, `irena`, `branko` -- each with $500 wallets.

### Other commands

```bash
./dev.sh down          # Stop everything
./dev.sh restart       # Clean restart + seed
./dev.sh logs          # Follow all logs
./dev.sh logs gateway  # Follow one service
./dev.sh prune         # Wipe everything (containers, volumes, images)
```

## Configuration

All secrets live in `.env` (gitignored). The `.env.example` template shows every variable:

| Variable | Used by | Purpose |
|----------|---------|---------|
| `JWT_SECRET_KEY` | All services | JWT token signing (min 32 chars) |
| `JWT_ISSUER` / `JWT_AUDIENCE` | All services | Token validation |
| `INTERNAL_API_KEY` | Account, Video, Donation | Service-to-service auth |
| `POSTGRES_USER` / `POSTGRES_PASSWORD` | All databases | DB credentials |
| `RABBITMQ_USER` / `RABBITMQ_PASS` | RabbitMQ + consumers | Broker credentials |
| `*_DB_CONNECTION_STRING` | Each service | Full Npgsql connection string |

Services read from environment variables first, then fall back to `appsettings.json`.

## Security

- **CORS**: All services restrict origins to a configurable whitelist (default: `localhost:5173`, `:3000`, `:50653`, `:50654`)
- **JWT**: BCrypt password hashing, 7-day token expiry, validated on all services
- **Inter-service auth**: gRPC and HTTP calls between services require `INTERNAL_API_KEY` header
- **Secrets**: No credentials in code or config files -- all in `.env` (gitignored)

## Search

The platform supports searching across videos and channels from a single search bar.

**Backend**: `GET /api/videos/search?q={query}&category={category}`
- Searches video titles and descriptions (case-insensitive, DB-level filtering)
- Optional `category` parameter to narrow results
- Returns videos with resolved uploader usernames (via gRPC)

## Swagger docs

| Service | URL |
|---------|-----|
| Account | http://localhost:5001/swagger |
| Video Catalogue | http://localhost:5002/swagger |
| Donation | http://localhost:5100 |
| Chat | http://localhost:5004/swagger |

## Testing

Integration tests cover all four services (Account, Video Catalogue, Donation, Chat) using `WebApplicationFactory` with EF Core InMemory databases and mocked external dependencies (RabbitMQ, gRPC, HTTP clients). No Docker or real databases are needed to run them.

```bash
# Run all tests
dotnet test

# Run tests for a single service
dotnet test tests/AccountService.IntegrationTests

# Use the PowerShell runner for formatted output
./scripts/run_integration_tests.ps1
./scripts/run_integration_tests.ps1 -Project Account
./scripts/run_integration_tests.ps1 -Filter "FullyQualifiedName~Auth"
```

Test projects live under `tests/` and share a common `Glense.TestUtilities` library for JWT token generation and service configuration helpers.

## Development

```bash
# Setup pre-commit hook for C# formatting
./scripts/setup-hooks.sh

# Setup git pp branch prefixes
./scripts/setup-pp.sh yourname
```

PRs require at least one approval before merge.
