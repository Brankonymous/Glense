# Glense

A microservice-based video streaming platform built with .NET 8, React, and PostgreSQL.

## Architecture

All frontend requests go through the **API Gateway** ([YARP](https://microsoft.github.io/reverse-proxy/) reverse proxy, port 5050), which routes to the appropriate microservice based on URL path. Services communicate with each other via HTTP.

```
                      ┌─────────────────┐
                      │    Frontend      │
                      │   (React/Vite)   │
                      └────────┬─────────┘
                               │
                    ┌──────────▼──────────┐
                    │   API Gateway       │
                    │   YARP :5050        │
                    │                     │
                    │ /api/auth/*     ──→ account   │
                    │ /api/profile/*  ──→ account   │
                    │ /api/videos/*   ──→ video     │
                    │ /api/donation/* ──→ donation  │
                    │ /api/chats/*    ──→ chat      │
                    │ /hubs/chat      ──→ chat (WS) │
                    └──┬──┬──┬──┬────────┘
         ┌────────────┘  │  │  └────────────┐
         ▼               ▼  ▼               ▼
  ┌────────────┐  ┌──────────┐  ┌────────────┐
  │  Account   │  │ Donation │  │   Video    │
  │   :5001    │  │  :5100   │  │ Catalogue  │
  │            │  │          │  │   :5002    │
  │ Auth       │  │ Wallets  │  │ Upload     │
  │ Profiles   │  │ Donations│  │ Comments   │
  │ Notifs     │  │          │  │ Playlists  │
  └──────┬─────┘  └────┬─────┘  └─────┬─────┘
         │    ▲         │   ▲          │
         │    └─────────┘   │          │
         │  wallet create   │          │
         │  on register     │          │
         │                  │          │
         │  validate user   │          │
         │  + notify        │          │
         │◄─────────────────┘          │
         │                             │
         │  resolve uploader username  │
         │◄────────────────────────────┘
         │
  ┌──────▼─────┐
  │   Chat     │
  │   :5004    │
  │            │
  │ Rooms      │
  │ Messages   │
  │ SignalR    │
  └────────────┘
```

The gateway is config-driven — adding a new route is a few lines of JSON in `appsettings.json`, not a new controller. YARP handles header forwarding, WebSocket proxying (SignalR), and active health checks automatically.

### Services and ports

| Service | Port | Database | Description |
|---------|------|----------|-------------|
| API Gateway (YARP) | 5050 | — | Routes all frontend requests, health checks backends |
| Account | 5001 | PostgreSQL :5432 | Auth, profiles, notifications |
| Donation | 5100 | PostgreSQL :5434 | Wallets and donations |
| Video Catalogue | 5002 | PostgreSQL :5433 | Video upload, comments, playlists |
| Chat | 5004 | PostgreSQL :5435 | Chat rooms, messages, SignalR |

### Inter-service communication

| Flow | Direction | Description |
|------|-----------|-------------|
| User registration | Account -> Donation | Auto-creates a wallet for the new user |
| Donation | Donation -> Account | Validates recipient exists, sends notification |
| Video listing | Video -> Account | Resolves uploader usernames |
| Chat messages | JWT -> Chat | Username extracted from JWT token |

Secondary operations (wallet creation, notifications) are non-blocking.

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
