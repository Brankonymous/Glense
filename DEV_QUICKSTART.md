# Dev Quickstart

## Prerequisites

- .NET 8 SDK
- Node.js v22
- Docker Desktop **or** Podman

## Start everything

### 1. Start databases + microservices

```bash
# Docker Desktop (recommended):
docker compose up --build postgres_account postgres_donation account_service donation_service -d

# Or Podman:
podman machine start
# Copy the DOCKER_HOST export line from podman's output, then:
docker compose up --build postgres_account postgres_donation account_service donation_service -d
```

### 2. Start the API Gateway (new terminal)

```bash
cd Glense.Server
dotnet run --urls http://localhost:5050
```

### 3. Start the frontend (new terminal)

```bash
cd glense.client
npm install
npm run dev
```

### 4. Seed test users (new terminal)

```bash
./scripts/seed-test-users.sh
```

Creates 3 users (password for all: `Password123!`):

| Username | Email | Type | Wallet |
|----------|-------|------|--------|
| keki | keki@glense.test | creator | $500 |
| irena | irena@glense.test | creator | $500 |
| branko | branko@glense.test | user | $500 |

## Ports

| Service | Port | Notes |
|---------|------|-------|
| Frontend (Vite) | 5173+ | Opens next free port |
| API Gateway | 5050 | All frontend requests go here |
| Account Service | 5001 | Auth, profiles, notifications |
| Donation Service | 5100 | Wallets, donations |
| PostgreSQL (Account) | 5432 | |
| PostgreSQL (Donation) | 5434 | |

## Quick test with curl

```bash
# Health checks
curl http://localhost:5050/health
curl http://localhost:5001/health
curl http://localhost:5100/health

# Register a user
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Password123!","confirmPassword":"Password123!","accountType":"user"}'

# Login
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"test","password":"Password123!"}'

# Search users
curl "http://localhost:5050/api/profile/search?q=keki"

# Check wallet (replace with real user ID from register/login response)
curl http://localhost:5050/api/wallet/user/USER_ID

# Send donation (replace IDs)
curl -X POST http://localhost:5050/api/donation \
  -H "Content-Type: application/json" \
  -d '{"donorUserId":"DONOR_ID","recipientUserId":"RECIPIENT_ID","amount":10,"message":"Nice!"}'
```

## Stop everything

```bash
docker compose down     # stop containers
# Ctrl+C on gateway and frontend terminals
podman machine stop     # if using podman
```

## Swagger docs

| Service | URL |
|---------|-----|
| Gateway | http://localhost:5050/swagger |
| Account Service | http://localhost:5001/swagger |
| Donation Service | http://localhost:5100 |
