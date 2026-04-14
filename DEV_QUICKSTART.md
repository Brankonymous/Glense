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

---

## Kubernetes (local)

### Prerequisites

```bash
brew install minikube kompose
```

Also requires Docker Desktop to be running.

### Setup

**Mac/Linux** — run from the project root:
```bash
./start.sh
```

**Windows** — open PowerShell as Administrator and run:
```powershell
.\start.ps1
```

Both scripts will automatically install missing tools (minikube, kubectl, kompose), build all images, apply manifests, seed test data, and start the port-forward.

> **Windows note:** `winget` is required (comes with Windows 11, or install [App Installer](https://apps.microsoft.com/detail/9NBLGGH4NNS1) on Windows 10). Git Bash or WSL is needed for the seed script.

If you prefer to run steps manually:
```bash
# 1. Start the cluster and build all service images
cd k8s && ./deploy.sh

# 2. In a separate terminal, expose the gateway
kubectl port-forward service/gateway 5050:5050

# 3. Seed test data (from project root)
cd .. && ./scripts/seed.sh
```

### Useful commands

```bash
kubectl get pods                          # check status of all pods
kubectl logs deployment/account-service  # view logs for a service
kubectl rollout restart deployment/gateway  # restart a deployment
```

### Rebuild after code changes

```bash
eval $(minikube docker-env)
docker build -t <service-name> ./path/to/service
kubectl rollout restart deployment/<service-name>
```

### Stop

```bash
minikube stop        # pause the cluster
minikube delete      # delete the cluster and all data
```

### Troubleshooting

**Postgres pods in CrashLoopBackOff**

This happens when the cluster is restarted and the persistent volumes contain stale data. Fix by wiping the volumes and restarting:

```bash
kubectl delete pvc postgres-account-data postgres-video-data postgres-donation-data postgres-chat-data
kubectl rollout restart deployment/postgres-account deployment/postgres-video deployment/postgres-donation deployment/postgres-chat
```

Wait ~15 seconds, then check `kubectl get pods` — all postgres pods should be `Running`.

**port-forward stops working after a deployment restart**

Kill the port-forward (`Ctrl+C`) and restart it:

```bash
kubectl port-forward service/gateway 5050:5050
```
