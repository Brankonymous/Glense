# Flow: Donation

## Trigger

`POST /api/donation { donorUserId, recipientUserId, amount, message }` (JWT required).

## Sequence

```mermaid
sequenceDiagram
    participant FE as Donations.jsx
    participant GW as Gateway
    participant DN as Donation
    participant AC as Account (REST)
    participant DBD as Postgres (donation)
    participant MQ as RabbitMQ
    participant ACC as Account (consumer)
    participant DBA as Postgres (account)

    FE->>GW: POST /api/donation { donor, recipient, amount, message }
    GW->>DN: forward (JWT)

    DN->>DN: validate amount > 0, donor != recipient
    DN->>AC: GET /api/profile/{recipientId}
    alt recipient not found
        AC-->>DN: 404
        DN-->>FE: 400 "Recipient user not found"
    else found
        AC-->>DN: 200 (also fetch donor username)

        DN->>DBD: BEGIN TX
        DN->>DBD: SELECT donor wallet
        alt insufficient
            DN-->>FE: 400 "Insufficient funds"
        else
            DN->>DBD: SELECT or INSERT recipient wallet
            DN->>DBD: UPDATE donor.balance -= amount
            DN->>DBD: UPDATE recipient.balance += amount
            DN->>DBD: INSERT donation
            DN->>DBD: COMMIT
            DN->>MQ: publish DonationMadeEvent { DonorId, RecipientId, Amount, DonorUsername }
            DN-->>FE: 201 DonationResponse
        end
    end

    MQ->>ACC: deliver DonationMadeEvent
    ACC->>DBA: INSERT notification { user_id=RecipientId, type='donation', message='DonorUsername sent $Amount' }
```

## Code refs

- Controller: [DonationController.cs](../../Glense.Server/DonationService/Controllers/DonationController.cs) (`CreateDonation`)
- Recipient lookup: `IAccountServiceClient` in [Glense.Server/DonationService/Services/](../../Glense.Server/DonationService/Services)
- Event: [Glense.Server/DonationService/Messages/DonationMadeEvent.cs](../../Glense.Server/DonationService/Messages/DonationMadeEvent.cs)
- Consumer: [services/Glense.AccountService/Consumers/DonationMadeEventConsumer.cs](../../services/Glense.AccountService/Consumers/DonationMadeEventConsumer.cs)

## Failure modes

| Failure | Behavior |
|---------|----------|
| Account unreachable for recipient validation | Donation rejected (`400`); no debit happens. |
| DB error mid-transaction | Rollback (Postgres) — no partial transfer. The in-memory provider does not support transactions; tests rely on single-`SaveChanges` atomicity. |
| RabbitMQ down at publish-time | Donation succeeds; warning logged. The recipient won't get a notification (acceptable — value already transferred). |
| Recipient has no wallet | Auto-created with balance 0 and credited. |

## Related APIs

- [`POST /api/donation`](../api/donation.md)
- [`GET /api/wallet/user/{userId}`](../api/donation.md)
- [`GET /api/notification`](../api/account.md)
