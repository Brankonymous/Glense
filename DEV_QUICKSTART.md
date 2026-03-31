# Dev Quickstart

## Prerequisites

- .NET 8 SDK (for running services outside Docker)
- Node.js v22
- Docker Desktop or Podman

## Setup

```bash
# 1. Copy environment file
cp .env.example .env

# 2. Start infrastructure + services
docker compose up --build -d

# 3. Start frontend
cd glense.client && npm install && npm run dev

# 4. Seed test data
./scripts/seed.sh
```

## Test users

Password for all: `Password123!`

| Username | Email | Type | Wallet |
|----------|-------|------|--------|
| keki | keki@glense.test | creator | $500 |
| irena | irena@glense.test | creator | $500 |
| branko | branko@glense.test | user | $500 |

## Ports

| Service | Port | Notes |
|---------|------|-------|
| Frontend (Vite) | 5173 | Opens next free port if taken |
| API Gateway | 5050 | All frontend requests go here |
| Account Service | 5001 (REST), 5003 (gRPC) | Auth, profiles, notifications |
| Video Catalogue | 5002 | Upload, comments, playlists |
| Donation Service | 5100 | Wallets, donations |
| Chat Service | 5004 | Rooms, messages, SignalR |
| RabbitMQ Management | 15672 | Default: guest/guest (override in .env) |
| PostgreSQL (Account) | 5432 | |
| PostgreSQL (Video) | 5433 | |
| PostgreSQL (Donation) | 5434 | |
| PostgreSQL (Chat) | 5435 | |

## Quick test

```bash
# Health checks
curl http://localhost:5050/health
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5100/health
curl http://localhost:5004/health

# Register
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Password123!"}'

# Login
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"test","password":"Password123!"}'

# Use the token from login response:
TOKEN="<paste token here>"

# Profile
curl http://localhost:5050/api/profile/me -H "Authorization: Bearer $TOKEN"

# Search users
curl "http://localhost:5050/api/profile/search?q=keki"
```

## Swagger docs

| Service | URL |
|---------|-----|
| Account | http://localhost:5001/swagger |
| Video Catalogue | http://localhost:5002/swagger |
| Donation | http://localhost:5100 |
| Chat | http://localhost:5004/swagger |

## Stop

```bash
docker compose down          # stop containers
docker compose down -v       # stop + wipe database volumes
```
