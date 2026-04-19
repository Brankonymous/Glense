# Dev Quickstart

## Prerequisites

- Docker Desktop (running)
- Python 3 on your PATH
- `minikube`, `kubectl`, `kompose` — install via `brew` on macOS, `winget install -e --id Kubernetes.<tool>` on Windows, or your distro's package manager on Linux

## Setup (Kubernetes — default)

From the project root:

```
# Direct (any OS)
python3 scripts/start.py

# Or the OS-specific wrapper
./start_unix.sh               # Mac / Linux
.\start_windows.ps1           # Windows PowerShell
```

This builds all images into minikube's local Docker daemon, applies the K8s manifests, starts `kubectl port-forward` for the gateway and frontend, waits for everything to be ready, and seeds test data. Takes a few minutes the first time.

### Access URLs

- Frontend: http://localhost:3000
- API Gateway: http://localhost:5050

### Re-seed only (cluster already running)

```
python3 scripts/seed.py
```

### Iterating after a code change

Images use `imagePullPolicy: Never`, so you rebuild into minikube's local Docker and restart the deployment.

**Once per shell session**, point Docker at minikube's daemon:

```
# Mac/Linux
eval $(minikube docker-env)
# Windows PowerShell
& minikube docker-env --shell powershell | Invoke-Expression
```

Then for whatever you changed:

```
docker build -t <image> <path>
kubectl rollout restart deployment/<name>
```

| Changed | Image | Path | Deployment |
|---------|-------|------|------------|
| Account service | `account-service` | `services/Glense.AccountService` | `account-service` |
| Video service | `video-service` | `services/Glense.VideoCatalogue` | `video-service` |
| Donation service | `donation-service` | `Glense.Server/DonationService` | `donation-service` |
| Chat service | `chat-service` | `services/Glense.ChatService` | `chat-service` |
| Gateway | `gateway` | `Glense.Server` | `gateway` |
| Frontend | `frontend` | `glense.client` | `frontend` |

### Useful commands

```
kubectl get pods                             # pod status
kubectl logs deployment/<name>               # service logs
kubectl describe pod <pod>                   # why a pod isn't Ready
kubectl rollout restart deployment/<name>    # restart a deployment
```

### Stop / reset

```
minikube stop                  # pause (data kept)
minikube delete                # delete the cluster
minikube delete --all --purge  # nuke everything and start fresh
```

### Troubleshooting

- Stuck pod? `kubectl describe pod <pod>` usually explains it.
- Minikube won't start? Raise Docker Desktop's memory (Settings → Resources) to 6 GB+, then `minikube delete --all --purge`.
- Port-forward died after a deployment restart? Re-run the start script, or `kubectl port-forward service/gateway 5050:5050` manually.

## Test users

Password for all: `Password123!`

| Username | Email | Type | Wallet |
|----------|-------|------|--------|
| keki | keki@glense.test | creator | $500 |
| irena | irena@glense.test | creator | $500 |
| branko | branko@glense.test | user | $500 |

## Quick test (via the gateway)

```bash
# Health check
curl http://localhost:5050/health

# Register
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"test","email":"test@test.com","password":"Password123!","confirmPassword":"Password123!","accountType":"user"}'

# Login
curl -X POST http://localhost:5050/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"test","password":"Password123!"}'

# Authenticated endpoints — paste token from login:
TOKEN="<paste token here>"
curl http://localhost:5050/api/profile/me -H "Authorization: Bearer $TOKEN"
curl "http://localhost:5050/api/profile/search?q=keki"
```

---

## Alternative: docker-compose (no Kubernetes)

Useful if you don't want to run a local K8s cluster. Exposes each service on its own host port.

```
# 1. Copy environment file (once)
cp .env.example .env            # Mac/Linux
Copy-Item .env.example .env     # Windows

# 2. Start infrastructure + services
docker compose up --build -d

# 3. Start frontend (separate terminal)
cd glense.client && npm install && npm run dev

# 4. Seed test data
python3 scripts/seed.py
```

### Ports (docker-compose only)

| Service | Port |
|---------|------|
| Frontend (Vite) | 5173 |
| API Gateway | 5050 |
| Account Service | 5001 (REST), 5003 (gRPC) |
| Video Catalogue | 5002 |
| Donation Service | 5100 |
| Chat Service | 5004 |
| RabbitMQ Management | 15672 (guest/guest) |
| PostgreSQL (Account / Video / Donation / Chat) | 5432 / 5433 / 5434 / 5435 |

### Swagger

| Service | URL |
|---------|-----|
| Account | http://localhost:5001/swagger |
| Video Catalogue | http://localhost:5002/swagger |
| Donation | http://localhost:5100/swagger |
| Chat | http://localhost:5004/swagger |

### Stop

```
docker compose down      # stop containers
docker compose down -v   # stop + wipe DB volumes
```
