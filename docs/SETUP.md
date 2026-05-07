# Setup

Canonical setup for the Glense platform. For the 60-second version, see [DEV_QUICKSTART.md](../DEV_QUICKSTART.md).

## Prerequisites

| Tool | Version | Why |
|------|---------|-----|
| .NET SDK | 8.0 | Build/run all backend services |
| Node.js | 22.x | Frontend (Vite + React 18) |
| Docker Desktop **or** Podman | latest | Container runtime for Postgres, RabbitMQ, services |
| `curl` (or PowerShell `Invoke-WebRequest`) | any | Health checks and smoke tests |

## 1. Environment file

Copy the template and edit if needed (defaults work for local dev):

```bash
cp .env.example .env
```

Every variable, where it's read, and its default is documented in [CONFIGURATION.md](CONFIGURATION.md).

## 2. Run the stack

### Option A — `dev.sh` (recommended)

```bash
./dev.sh           # build + start + wait-for-health + seed
./dev.sh down      # stop everything
./dev.sh restart   # clean restart + reseed
./dev.sh logs      # follow all logs
./dev.sh logs gateway
./dev.sh prune     # wipe containers, volumes, images
```

Auto-detects `podman-compose`, `podman compose`, or `docker compose`. Source: [dev.sh](../dev.sh).

### Option B — Compose directly

```bash
docker compose up --build -d
```

### Option C — Run a service outside containers

Useful when iterating on one service while the others run in containers.

```bash
# Stop just that container so the host process can take its port:
docker compose stop account_service

cd services/Glense.AccountService
dotnet run
```

The service reads env vars from a `.env` file in the current directory or any parent — this is implemented in [services/Glense.AccountService/Program.cs](../services/Glense.AccountService/Program.cs) and [services/Glense.ChatService/Program.cs](../services/Glense.ChatService/Program.cs).

## 3. Frontend

```bash
cd glense.client
npm install
npm run dev
```

Vite serves on http://localhost:5173 (or the next free port). It expects the gateway at http://localhost:5050 by default — see [glense.client/README.md](../glense.client/README.md) for env vars.

## 4. Seed data

```bash
./scripts/seed.sh
```

Creates three users (password `Password123!`):

| Username | Email | Type | Wallet |
|----------|-------|------|--------|
| `keki`   | keki@glense.test   | creator | $500 |
| `irena`  | irena@glense.test  | creator | $500 |
| `branko` | branko@glense.test | user    | $500 |

Plus sample videos and chats. See [scripts/README.md](../scripts/README.md).

## 5. Verify

```bash
curl http://localhost:5050/health   # Gateway
curl http://localhost:5001/health   # Account (REST)
curl http://localhost:5002/health   # Video Catalogue
curl http://localhost:5100/health   # Donation
curl http://localhost:5004/health   # Chat
```

End-to-end smoke test:

```bash
curl -X POST http://localhost:5050/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"smoke","email":"smoke@test.com","password":"Password123!","confirmPassword":"Password123!","accountType":"user"}'
```

## Ports (host → container)

| Service | Host port | Container port | Notes |
|---------|----------:|---------------:|-------|
| Gateway | 5050 | 5050 | All frontend traffic |
| Account REST | 5001 | 5000 | Mapped via `ACCOUNT_REST_PORT` |
| Account gRPC | 5003 | 5001 | HTTP/2 cleartext, `ACCOUNT_GRPC_PORT` |
| Video Catalogue | 5002 | 5002 | 500 MB upload limit |
| Donation | 5100 | 5100 | Port set by `PORT` env var |
| Chat | 5004 | 5000 | SignalR hub at `/hubs/chat` |
| Frontend (containerized) | 3000 | 3000 | Optional — `dev.sh` doesn't start it |
| Postgres (account) | 5432 | 5432 | `glense_account` DB |
| Postgres (video) | 5433 | 5432 | `glense_video` DB |
| Postgres (donation) | 5434 | 5432 | `glense_donation` DB |
| Postgres (chat) | 5435 | 5432 | `glense_chat` DB |
| RabbitMQ | 5672 / 15672 | 5672 / 15672 | AMQP / management UI |

Source of truth: [docker-compose.yml](../docker-compose.yml).

## Swagger

| Service | URL |
|---------|-----|
| Account | http://localhost:5001/swagger |
| Video Catalogue | http://localhost:5002/swagger |
| Donation | http://localhost:5100/ (Swagger UI is at root) |
| Chat | http://localhost:5004/swagger |

## Troubleshooting

**Port already in use.** `dev.sh` auto-kills stale `dotnet`/`Glense*` processes on service ports. If a non-dotnet process owns the port, free it manually:

```powershell
Get-NetTCPConnection -LocalPort 5050 | Select-Object -First 1 OwningProcess
Stop-Process -Id <pid>
```

**RabbitMQ slow to start.** Compose `healthcheck` retries for ~50 s. If services start before RabbitMQ, MassTransit reconnects automatically.

**EF migrations.** None of the services use migrations; Account/Video/Donation/Chat all call `Database.EnsureCreated()` on startup (or apply the SQL in `services/Glense.AccountService/database/schema.sql` when running Postgres in a container).

**Account gRPC unreachable from Video.** In compose, the Video service uses `ACCOUNT_GRPC_URL=http://account_service:5001` (container-internal). If you run Video on the host, override to `http://localhost:5003`.

**Frontend can't reach the gateway.** Confirm `VITE_API_URL` (or per-service `VITE_*_URL`) — see [glense.client/README.md](../glense.client/README.md). Confirm `Cors:AllowedOrigins` includes the Vite dev port.

**Wipe everything.** `./dev.sh prune` removes containers, volumes, and images.
