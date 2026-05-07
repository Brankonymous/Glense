# Donation API

Base URL (direct): `http://localhost:5100`. Through gateway: `http://localhost:5050`.

Auth: all endpoints require JWT.

Source: [Glense.Server/DonationService/Controllers/](../../Glense.Server/DonationService/Controllers).

## Wallets ([WalletController.cs](../../Glense.Server/DonationService/Controllers/WalletController.cs))

| Method | Path | Notes |
|--------|------|-------|
| `GET` | `/api/wallet/user/{userId:guid}` | Returns the wallet. `404` if not found. |
| `POST` | `/api/wallet` | Idempotent. Body `{ "userId": "<guid>", "initialBalance": 100.00 }`. Returns existing wallet on conflict. |
| `POST` | `/api/wallet/user/{userId:guid}/topup` | Body `{ "amount": 50.0 }`. `400` if amount ≤ 0. |
| `POST` | `/api/wallet/user/{userId:guid}/withdraw` | Body `{ "amount": 25.0 }`. `400` if amount ≤ 0 or insufficient funds. |

`WalletResponse`:
```json
{ "id": "<guid>", "userId": "<guid>", "balance": 500.00, "createdAt": "...", "updatedAt": "..." }
```

## Donations ([DonationController.cs](../../Glense.Server/DonationService/Controllers/DonationController.cs))

| Method | Path | Notes |
|--------|------|-------|
| `GET` | `/api/donation/donor/{userId:guid}` | List donations sent by a user, newest first |
| `GET` | `/api/donation/recipient/{userId:guid}` | List donations received |
| `POST` | `/api/donation` | Create donation, debit donor wallet, credit recipient wallet, publish `DonationMadeEvent` |

`POST /api/donation` body:
```json
{
  "donorUserId": "<guid>",
  "recipientUserId": "<guid>",
  "amount": 25.00,
  "message": "Great content!"
}
```

Validation order:
1. `amount > 0` — else `400`.
2. `donor != recipient` — else `400`.
3. Recipient exists in Account (sync HTTP `GET /api/profile/{recipientId}`) — else `400`.
4. Donor wallet exists — else `404`.
5. Donor balance ≥ amount — else `400`.

Recipient wallet is auto-created if missing. Funds transfer happens inside a DB transaction (skipped only for in-memory provider). After commit, `DonationMadeEvent` is published; failure to publish does **not** fail the donation. See [flows/06-donation.md](../flows/06-donation.md).

`DonationResponse`:
```json
{ "id": "<guid>", "donorUserId": "<guid>", "recipientUserId": "<guid>", "amount": 25.00, "message": "...", "createdAt": "..." }
```

## Health

`GET /health` — JSON `{ status, service, timestamp }`.

## Inter-service

- HTTP → Account: recipient validation + donor username lookup (`IAccountServiceClient`).
- RabbitMQ consumer: `UserRegisteredEvent` → creates wallet (idempotent — see [UserRegisteredEventConsumer.cs](../../Glense.Server/DonationService/Consumers/UserRegisteredEventConsumer.cs)).
- RabbitMQ producer: `DonationMadeEvent`.
