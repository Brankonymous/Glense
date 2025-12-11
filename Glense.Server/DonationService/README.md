# Donation Microservice

A standalone microservice for handling donations and wallet management in the Glense platform.

## 🚀 Quick Start

### Local Development (In-Memory DB)

```bash
cd Glense.Server/DonationService
dotnet run
```

No database setup needed - runs with in-memory database by default.

### With Neon PostgreSQL

Set your Neon connection string:

```bash
export DONATION_DB_CONNECTION_STRING="Host=your-neon-host.neon.tech;Database=donation_db;Username=your-user;Password=your-password;SslMode=Require"
dotnet run
```

Or add it to `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DonationDb": "Host=your-neon-host.neon.tech;..."
  }
}
```

### Docker

```bash
# With Neon connection string
DONATION_DB_CONNECTION_STRING="your-neon-connection-string" docker-compose up -d

# View logs
docker-compose logs -f donation-service

# Stop
docker-compose down
```

## 📡 API Endpoints

### Donations

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/donation/donor/{userId}` | Get donations sent by user |
| GET | `/api/donation/recipient/{userId}` | Get donations received by user |
| POST | `/api/donation` | Create a new donation |

### Wallets

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/wallet/user/{userId}` | Get wallet by user ID |
| POST | `/api/wallet` | Create a new wallet |
| POST | `/api/wallet/user/{userId}/topup` | Add funds to wallet |

### Health & Documentation

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check endpoint |
| GET | `/` | Swagger UI documentation |

## 📋 Example Requests

### Create a Wallet
```bash
curl -X POST http://localhost:5100/api/wallet \
  -H "Content-Type: application/json" \
  -d '{"userId": 1, "initialBalance": 100.00}'
```

### Top Up Wallet
```bash
curl -X POST http://localhost:5100/api/wallet/user/1/topup \
  -H "Content-Type: application/json" \
  -d '{"amount": 50.00}'
```

### Make a Donation
```bash
curl -X POST http://localhost:5100/api/donation \
  -H "Content-Type: application/json" \
  -d '{
    "donorUserId": 1,
    "recipientUserId": 2,
    "amount": 25.00,
    "message": "Great content!"
  }'
```

### Get Wallet
```bash
curl http://localhost:5100/api/wallet/user/1
```

## 🔧 Configuration

| Variable | Description | Default |
|----------|-------------|---------|
| `PORT` | Service port | `5100` |
| `DONATION_DB_CONNECTION_STRING` | Neon PostgreSQL connection string | In-memory DB |
