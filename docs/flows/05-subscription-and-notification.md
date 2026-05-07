# Flow: Subscription & notification

## Trigger

User clicks Subscribe on a channel page.

## Sequence

```mermaid
sequenceDiagram
    participant FE as ChannelDetail.jsx
    participant GW as Gateway
    participant VC as Video Catalogue
    participant DBV as Postgres (video)
    participant AC2 as Account (gRPC)
    participant MQ as RabbitMQ
    participant AC as Account (consumer)
    participant DBA as Postgres (account)

    FE->>GW: POST /api/subscriptions { subscribedToId }
    GW->>VC: forward (JWT)
    VC->>DBV: check existing subscription
    alt already subscribed
        VC-->>FE: 409 Conflict
    else not subscribed
        VC->>DBV: INSERT subscription
        VC->>AC2: gRPC GetUsername(subscriberId)
        VC->>MQ: publish UserSubscribedEvent { SubscriberId, ChannelOwnerId, SubscriberUsername }
        VC-->>FE: 201 SubscribeResponseDTO
    end

    MQ->>AC: deliver UserSubscribedEvent
    AC->>DBA: INSERT notification { user_id=ChannelOwnerId, type='subscription', message='SubscriberUsername subscribed' }
```

## Code refs

- Producer: [SubscriptionsController.cs](../../services/Glense.VideoCatalogue/Controllers/SubscriptionsController.cs) (`Subscribe`)
- Event: [services/Glense.VideoCatalogue/Messages/UserSubscribedEvent.cs](../../services/Glense.VideoCatalogue/Messages/UserSubscribedEvent.cs)
- Consumer: [services/Glense.AccountService/Consumers/UserSubscribedEventConsumer.cs](../../services/Glense.AccountService/Consumers/UserSubscribedEventConsumer.cs)

## Failure modes

| Failure | Behavior |
|---------|----------|
| RabbitMQ down at publish-time | Subscription still recorded; warning logged. The notification is **lost** in this corner case (no outbox). |
| Account consumer down | Event waits in queue, picked up on recovery. |
| gRPC username lookup fails | `SubscriberUsername` defaults to `"Someone"`. |

## Related APIs

- [`POST /api/subscriptions`](../api/video-catalogue.md)
- [`GET /api/notification`](../api/account.md)
