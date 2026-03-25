# Glense

A microservice-based video streaming platform built with .NET 8, React, and PostgreSQL.

## Architecture

All frontend requests go through the **API Gateway** (port 5050), which proxies to the appropriate microservice. Services communicate with each other via HTTP.

```
                          ┌─────────────────┐
                          │    Frontend      │
                          │   (React/Vite)   │
                          └────────┬─────────┘
                                   │
                          ┌────────▼─────────┐
                          │   API Gateway     │
                          │     :5050         │
                          └──┬──┬──┬──┬──────┘
                  ┌──────────┘  │  │  └──────────┐
                  ▼             ▼  ▼             ▼
          ┌──────────────┐ ┌────────────┐ ┌──────────────┐
          │   Account    │ │  Donation  │ │    Video     │
          │   :5001      │ │   :5100    │ │  Catalogue   │
          │              │ │            │ │    :5002     │
          │ Auth/Profile │ │  Wallets   │ │   Upload     │
          │ Notification │ │  Donations │ │  Comments    │
          └──────┬───────┘ └─────┬──────┘ │  Playlists   │
                 │    ▲          │   ▲    └──────┬───────┘
                 │    └──────────┘   │           │
                 │    wallet create  │           │
                 │    on register    │           │
                 │                   │           │
                 │    validate user  │           │
                 │    + notify       │           │
                 │                   │           │
                 │◄──────────────────┘           │
                 │                               │
                 │   resolve uploader username   │
                 │◄──────────────────────────────┘
                 │
          ┌──────▼───────┐
          │    Chat      │
          │    :5004     │
          │              │
          │  Rooms       │
          │  Messages    │
          │  SignalR     │
          └──────────────┘
```

### Inter-service communication

| Flow | Direction | Description |
|------|-----------|-------------|
| User registration | Account → Donation | Auto-creates a wallet for the new user |
| Donation | Donation → Account | Validates recipient exists, sends notification after success |
| Video listing | Video → Account | Resolves uploader usernames for video responses |
| Chat messages | JWT → Chat | Username extracted from JWT token, stored with message |

Secondary operations (wallet creation, notifications) are non-blocking — if a downstream service is unavailable, the primary operation still succeeds.

### Services and ports

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| API Gateway | 5050 | — | Proxies all frontend requests |
| Account | 5001 | PostgreSQL :5432 | Auth, profiles, notifications |
| Donation | 5100 | PostgreSQL :5434 | Wallets and donations |
| Video Catalogue | 5002 | PostgreSQL :5433 | Video upload, comments, playlists |
| Chat | 5004 | PostgreSQL :5435 | Chat rooms, messages, SignalR |

## Quick start

See [DEV_QUICKSTART.md](DEV_QUICKSTART.md) for full setup instructions.

```bash
# Start all services
docker compose up --build -d

# Start the gateway
cd Glense.Server && dotnet run --urls http://localhost:5050

# Start the frontend
cd glense.client && npm install && npm run dev

# Seed test data (users, videos, donations, comments)
./scripts/seed.sh
```

## Prerequisites

- .NET 8 SDK
- Node.js v22
- Docker or Podman

## Database schema

Each microservice owns its own database. See individual service READMEs for schema details.

![Glense Database Schema](schema-Glense.svg)

## Development workflow

1. Set up `git pp` so branch names are prefixed with your username
2. Use GitHub Issues for sprint tracking
3. PRs require at least one approval before merge

```bash
# Setup git pp (one-time)
./scripts/setup-pp.sh yourname

# Setup pre-commit hook for C# formatting (one-time)
./scripts/setup-hooks.sh
```

The pre-commit hook auto-formats C# code. If files are modified by formatting, stage them and commit again.
