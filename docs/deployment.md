# Deployment

Two supported deployment modes:

| Mode | When to use | Entry point |
|------|-------------|-------------|
| Docker Compose | Quick local dev, no cluster overhead | [../docker-compose.yml](../docker-compose.yml), `docker compose up --build -d` |
| Kubernetes (minikube) | Prod-like, multi-replica testing | [../k8s/](../k8s/), `python3 scripts/start.py` |

For the step-by-step setup walkthrough, see [../DEV_QUICKSTART.md](../DEV_QUICKSTART.md).

## Docker Compose ports

| Service | Port |
|---------|------|
| Frontend (Vite) | 5173 |
| API Gateway | 5050 |
| Account Service | 5001 (REST), 5003 (gRPC) |
| Video Catalogue | 5002 |
| Donation Service | 5100 |
| Chat Service | 5004 |
| RabbitMQ Management | 15672 (guest/guest) |
| Postgres (Account / Video / Donation / Chat) | 5432 / 5433 / 5434 / 5435 |

## Kubernetes layout

Manifests live in [../k8s/](../k8s/):

| Deployment | Service manifest | Image | Source path |
|------------|------------------|-------|-------------|
| `gateway` | `gateway-service.yaml` | `gateway` | [../Glense.Server](../Glense.Server) |
| `account-service` | `account_service-service.yaml` | `account-service` | [../services/Glense.AccountService](../services/Glense.AccountService) |
| `video-service` | `video_service-service.yaml` | `video-service` | [../services/Glense.VideoCatalogue](../services/Glense.VideoCatalogue) |
| `donation-service` | `donation_service-service.yaml` | `donation-service` | [../Glense.Server/DonationService](../Glense.Server/DonationService) |
| `chat-service` | `chat_service-service.yaml` | `chat-service` | [../services/Glense.ChatService](../services/Glense.ChatService) |
| `frontend` | `frontend-service.yaml` | `frontend` | [../glense.client](../glense.client) |
| `rabbitmq` | `rabbitmq-service.yaml` | `rabbitmq` (official) | — |
| `postgres-{account,chat,donation,video}` | `postgres_*-service.yaml` | `postgres` (official) | — |

Images use `imagePullPolicy: Never` and are built directly into minikube's local Docker daemon.

## Environment variables (per service)

### Gateway

| Variable | Purpose |
|----------|---------|
| `Cors__AllowedOrigins__0..N` | CORS allow-list |
| `ReverseProxy__Routes__*` | Route table (usually static in `appsettings.json`) |

### Account

| Variable | Purpose |
|----------|---------|
| `ACCOUNT_REST_PORT` | REST listener (in-container) |
| `ACCOUNT_GRPC_PORT` | gRPC listener (in-container) |
| `ConnectionStrings__DefaultConnection` | Postgres |
| `JWT_SECRET_KEY` / `JWT_ISSUER` / `JWT_AUDIENCE` | JWT issuance & validation |
| `INTERNAL_API_KEY` | Validates inbound gRPC + internal HTTP |
| `RabbitMQ__Host` / `RabbitMQ__Username` / `RabbitMQ__Password` | Broker |

### Video Catalogue

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__VideoCatalogue` | Postgres |
| `JwtSettings__SecretKey` / `Issuer` / `Audience` | JWT validation |
| `INTERNAL_API_KEY` | Header on outbound gRPC to Account |
| `AccountService__GrpcUrl` | Account gRPC URL |
| `RabbitMQ__*` | Broker |
| `VideoStorage__Path` | Where uploaded files are written |

### Donation

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__DonationDb` (or `DONATION_DB_CONNECTION_STRING`) | Postgres |
| `JwtSettings__SecretKey` / `Issuer` / `Audience` | JWT validation |
| `INTERNAL_API_KEY` | Header on outbound HTTP to Account |
| `AccountService__BaseUrl` | Account REST base URL |
| `RabbitMQ__*` | Broker |

### Chat

| Variable | Purpose |
|----------|---------|
| `ConnectionStrings__DefaultConnection` | Postgres |
| `JwtSettings__SecretKey` / `Issuer` / `Audience` | JWT validation |

### Frontend

| Variable | Purpose |
|----------|---------|
| `VITE_API_URL` | Gateway URL (build- and runtime config) |

## Secrets

In Kubernetes, secrets are currently inlined as `env` values on each deployment manifest (suitable for local minikube, **not** production). For a real deployment, replace them with `Secret` resources and `envFrom`.

`INTERNAL_API_KEY` and `JWT_SECRET_KEY` must be the same value across every service that uses them. The gateway does not need them.

## Health checks

Every service exposes `GET /health`. The Kubernetes manifests use these as readiness probes (see e.g. [../k8s/account-service-deployment.yaml](../k8s/account-service-deployment.yaml)).
