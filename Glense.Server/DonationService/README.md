# Donation Service

Wallets and donations.

## Tech stack

ASP.NET Core 8 · EF Core (Npgsql, in-memory dev fallback) · MassTransit/RabbitMQ · Swagger UI at `/`.

## Run

```bash
./dev.sh                     # full stack
# or
cd Glense.Server/DonationService
dotnet run                   # http://localhost:5100 (PORT env var)
```

Swagger: http://localhost:5100/.

## Endpoints (summary)

- Wallets: `GET /api/wallet/user/{id}`, `POST /api/wallet`, `POST /api/wallet/user/{id}/topup`, `POST /api/wallet/user/{id}/withdraw`
- Donations: `GET /api/donation/donor/{id}`, `GET /api/donation/recipient/{id}`, `POST /api/donation`
- `GET /health`

Full reference: [../../docs/api/donation.md](../../docs/api/donation.md). Donation transfer flow: [../../docs/flows/06-donation.md](../../docs/flows/06-donation.md).

## Inter-service

| Direction | Mechanism | Purpose |
|-----------|-----------|---------|
| → Account | HTTP `GET /api/profile/{id}` | Validate donation recipient + fetch usernames |
| ← Account | RabbitMQ `UserRegisteredEvent` | Auto-create wallet for new user |
| → Account | RabbitMQ `DonationMadeEvent` | Notify recipient |

## Configuration

See [../../docs/CONFIGURATION.md](../../docs/CONFIGURATION.md). Key per-service vars: `PORT`, `DONATION_DB_CONNECTION_STRING` (or `ConnectionStrings:DonationDb`), `ACCOUNT_SERVICE_URL`.

If no connection string is supplied, an EF Core in-memory database is used (development only — transactions are skipped automatically).

## Tests

[../../tests/DonationService.IntegrationTests/](../../tests/DonationService.IntegrationTests).
