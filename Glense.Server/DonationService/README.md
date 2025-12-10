# Donation Microservice

A simple read-only service for fetching wallet and donation data from Neon PostgreSQL.

## Database Schema

### `wallets`
| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| user_id | INT | User ID |
| balance | DECIMAL(12,2) | Current balance |
| created_at | TIMESTAMP | Creation time |
| updated_at | TIMESTAMP | Last update |

### `donations`
| Column | Type | Description |
|--------|------|-------------|
| id | UUID | Primary key |
| donor_user_id | INT | Who sent |
| recipient_user_id | INT | Who received |
| amount | DECIMAL(12,2) | Donation amount |
| message | TEXT | Optional message |
| created_at | TIMESTAMP | When donated |

## Setup

### With Neon (production)
1. Create a [Neon](https://neon.tech) database
2. Run `Data/schema.sql` in Neon SQL Editor
3. Set environment variable:
```
DONATION_DB_CONNECTION_STRING=Host=xxx.neon.tech;Database=neondb;Username=xxx;Password=xxx;SSL Mode=Require
```

### Without Neon (development)
Just run - uses in-memory database automatically.

## API

- `GET /api/wallet` - All wallets
- `GET /api/wallet/{userId}` - Wallet by user
- `GET /api/donation` - All donations
- `GET /api/donation/{donationId}` - Donation by ID

## Tests

```bash
# With Neon
export DONATION_DB_CONNECTION_STRING="..."
dotnet test DonationService.Tests

# Without Neon (in-memory)
dotnet test DonationService.Tests
```
