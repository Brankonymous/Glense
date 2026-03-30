# Glense

A microservice-based video streaming platform built with .NET 8, React, and PostgreSQL.

## Architecture

All frontend requests go through the **API Gateway** ([YARP](https://microsoft.github.io/reverse-proxy/) reverse proxy, port 5050), which routes to the appropriate microservice based on URL path.

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
                      │ /api/auth/*    → Account  │
                      │ /api/profile/* → Account  │
                      │ /api/videos/*  → Video    │
                      │ /api/donation/*→ Donation │
                      │ /api/chats/*   → Chat     │
                      │ /hubs/chat     → Chat(WS) │
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
           │◄─── RabbitMQ ──── Donation     │  wallet create on registration
           │◄─── HTTP ──────── Donation     │  validate recipient
           │◄─── RabbitMQ ──── Donation     │  donation notification
           │◄─── gRPC ──────────────── Video   resolve usernames
           │◄─── RabbitMQ ─────────── Video    subscription notification
```

The gateway is config-driven — adding a new route is a few lines of JSON in `appsettings.json`. YARP handles header forwarding, WebSocket proxying (SignalR), and active health checks.

### Services and ports

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| API Gateway (YARP) | 5050 | — | Routes all frontend requests, health checks backends |
| Account | 5001 (REST), 5003 (gRPC) | PostgreSQL :5432 | Auth, profiles, notifications, gRPC server |
| Donation | 5100 | PostgreSQL :5434 | Wallets and donations |
| Video Catalogue | 5002 | PostgreSQL :5433 | Video upload, comments, playlists |
| Chat | 5004 | PostgreSQL :5435 | Chat rooms, messages, SignalR |
| RabbitMQ | 5672 (AMQP), 15672 (management UI) | — | Message broker for async events |

### Inter-service communication

Services use three different protocols depending on the use case:

| Flow | Direction | Protocol | Why |
|------|-----------|----------|-----|
| Wallet creation | Account → Donation | **RabbitMQ** | Fire-and-forget event on registration |
| Donation notification | Donation → Account | **RabbitMQ** | Async notification, doesn't block donation |
| Subscription notification | Video → Account | **RabbitMQ** | Async notification on subscribe |
| Recipient validation | Donation → Account | **HTTP** | Synchronous check before processing |
| Username resolution | Video → Account | **gRPC** | High-performance batch lookups (Protobuf) |
| Chat auth | JWT → Chat | **JWT claims** | No inter-service call needed |

**RabbitMQ** (MassTransit) handles fire-and-forget events where the sender doesn't need a response. **gRPC** handles high-frequency synchronous lookups with binary serialization. **HTTP** is kept for simple synchronous calls.

## Quick start

```bash
# Start everything (containers + seed data)
./dev.sh

# Start the frontend (separate terminal)
cd glense.client && npm install && npm run dev
```

Other commands:

```bash
./dev.sh down      # Stop everything
./dev.sh restart   # Full clean restart + seed
./dev.sh logs      # Follow all container logs
./dev.sh logs gateway  # Follow a single service
./dev.sh seed      # Re-seed test data
./dev.sh prune     # Nuclear option: stop + wipe all images/cache
```

The seed script creates 3 users (password: `Password123!`), wallets, donations, 8 videos with comments.

### Manual setup

If you prefer to start services individually:

```bash
# Start databases, then services, then gateway
docker compose up --build -d postgres_account postgres_video postgres_donation postgres_chat
docker compose up --build -d account_service video_service donation_service chat_service gateway

# Seed test data
./scripts/seed.sh
```

Works with both Docker and Podman.

## Prerequisites

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
