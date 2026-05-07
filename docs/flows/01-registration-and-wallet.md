# Flow: Registration & wallet creation

## Trigger

Client `POST /api/auth/register` through the gateway.

## Sequence

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant GW as Gateway
    participant AC as Account Service
    participant DB as Postgres (account)
    participant MQ as RabbitMQ
    participant DN as Donation Service
    participant DBD as Postgres (donation)

    FE->>GW: POST /api/auth/register {username,email,password,...}
    GW->>AC: forward
    AC->>DB: INSERT INTO users (BCrypt hash)
    AC->>AC: issue JWT (HS256, 7d)
    AC->>MQ: publish UserRegisteredEvent {UserId, Username}
    AC-->>FE: 200 { token, user, expiresAt }
    Note over MQ,DN: asynchronous
    MQ->>DN: deliver UserRegisteredEvent
    DN->>DBD: SELECT wallet WHERE user_id (idempotency)
    alt no wallet
        DN->>DBD: INSERT wallet {userId, balance: 0}
    end
```

## Code refs

- HTTP entry: `AuthController.Register` in [services/Glense.AccountService/Controllers/AuthController.cs](../../services/Glense.AccountService/Controllers/AuthController.cs)
- Hashing + token: `AuthService` in [services/Glense.AccountService/Services/](../../services/Glense.AccountService/Services)
- Event type: [services/Glense.AccountService/Messages/UserRegisteredEvent.cs](../../services/Glense.AccountService/Messages/UserRegisteredEvent.cs)
- Consumer: [Glense.Server/DonationService/Consumers/UserRegisteredEventConsumer.cs](../../Glense.Server/DonationService/Consumers/UserRegisteredEventConsumer.cs)

## Failure modes

| Failure | Behavior |
|---------|----------|
| RabbitMQ down at register-time | User is still created. The publish is best-effort; once RabbitMQ recovers, MassTransit retries on the next bus interaction. |
| Donation service down | Event sits in the queue; wallet is created when Donation comes back. |
| Duplicate event delivery | Consumer checks for existing wallet (idempotent). |
| Wallet insert race | `DbUpdateException` caught and logged; the other writer wins. |

## Related APIs

- [`POST /api/auth/register`](../api/account.md)
- [`GET /api/wallet/user/{userId}`](../api/donation.md)
