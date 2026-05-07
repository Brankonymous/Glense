# Account Service — Standalone Quickstart

Quick recipe for running just the Account service. For the full-stack setup, use [../../DEV_QUICKSTART.md](../../DEV_QUICKSTART.md) instead.

## Run with Docker Compose

From the project root:

```bash
docker compose up account_service postgres_account
```

Service: `http://localhost:5001` · Swagger: `http://localhost:5001/swagger`.

## Run with `dotnet run`

```bash
cd services/Glense.AccountService
dotnet run
```

If `ConnectionStrings__DefaultConnection` is not set, the service uses EF InMemory.

## Smoke test

```bash
# Health
curl http://localhost:5001/health

# Register
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser","email":"test@example.com","password":"Password123!","confirmPassword":"Password123!","accountType":"user"}'

# Login
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usernameOrEmail":"testuser","password":"Password123!"}'
```

The full REST contract is in [ACCOUNT_API.md](ACCOUNT_API.md).
