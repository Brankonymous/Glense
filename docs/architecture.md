# Architecture

Glense is a microservice video platform composed of four backend services, an API gateway, a React SPA, and shared infrastructure (PostgreSQL per service, RabbitMQ).

For setup instructions see [../DEV_QUICKSTART.md](../DEV_QUICKSTART.md). For end-to-end flows see [flows.md](flows.md).

## Service map

```
                        ┌─────────────────┐
                        │    Frontend     │
                        │   (React/Vite)  │
                        └────────┬────────┘
                                 │ REST + WebSocket
                      ┌──────────▼──────────────┐
                      │      API Gateway        │
                      │      YARP :5050         │
                      │                         │
                      │ /api/auth/*    → Account │
                      │ /api/profile/* → Account │
                      │ /api/notification/* → Account │
                      │ /api/videos/*  → Video   │
                      │ /api/subscriptions/* → Video │
                      │ /api/videolikes/*    → Video │
                      │ /api/playlists/*     → Video │
                      │ /api/playlistvideos/*→ Video │
                      │ /api/donation/*→ Donation│
                      │ /api/wallet/*  → Donation│
                      │ /api/chats/*   → Chat    │
                      │ /api/messages/*→ Chat    │
                      │ /hubs/chat     → Chat(WS)│
                      └──┬───┬───┬───┬──────────┘
           ┌─────────────┘   │   │   └──────────┐
           ▼                 ▼   ▼              ▼
    ┌─────────────┐  ┌────────────┐  ┌─────────────┐  ┌─────────────┐
    │   Account   │  │  Donation  │  │    Video    │  │    Chat     │
    │    :5001    │  │   :5100    │  │  Catalogue  │  │    :5004    │
    │             │  │            │  │    :5002    │  │             │
    │ Auth        │  │ Wallets    │  │ Upload      │  │ Rooms       │
    │ Profiles    │  │ Donations  │  │ Comments    │  │ Messages    │
    │ Notifs      │  │            │  │ Playlists   │  │ SignalR hub │
    │ gRPC server │  │            │  │ gRPC client │  │             │
    └──────┬──────┘  └──────┬─────┘  └──────┬──────┘  └─────────────┘
           │                │               │
           │         ┌──────┴──────┐        │
           └─────────┤  RabbitMQ   ├────────┘
                     │ :5672/:15672│
                     └─────────────┘
```

## Responsibilities

| Component | Source | Owns |
|-----------|--------|------|
| API Gateway | [Glense.Server](../Glense.Server/) | YARP reverse proxy, CORS, gateway-level health |
| Account | [services/Glense.AccountService](../services/Glense.AccountService/) | Users, JWT issuance, profiles, notifications, gRPC username server |
| Video Catalogue | [services/Glense.VideoCatalogue](../services/Glense.VideoCatalogue/) | Videos, comments, subscriptions, likes, playlists, file storage |
| Donation | [Glense.Server/DonationService](../Glense.Server/DonationService/) | Wallets, donations |
| Chat | [services/Glense.ChatService](../services/Glense.ChatService/) | Chat rooms, messages, real-time delivery via SignalR |
| Frontend | [glense.client](../glense.client/) | React SPA, talks only to the gateway |

## Data ownership

Each service owns its own PostgreSQL database — no shared tables, no cross-database joins.

| Service | Database (compose port) | Tables |
|---------|------------------------|--------|
| Account | `glense_account` (5432) | `users`, `notifications` |
| Video Catalogue | `glense_video` (5433) | `Videos`, `Comments`, `Subscriptions`, `VideoLikes`, `Playlists`, `PlaylistVideos` |
| Donation | `glense_donation` (5434) | `Wallets`, `Donations` |
| Chat | `glense_chat` (5435) | `Chats`, `Messages` |

User identity is replicated by id only (UUID); other services never store usernames or emails. They resolve usernames on-demand via the Account gRPC API.

## Communication patterns

| Flow | Protocol | Why |
|------|----------|-----|
| Wallet creation on registration | Account → Donation, RabbitMQ `UserRegisteredEvent` | Fire-and-forget; registration must not depend on Donation being up |
| Donation notification | Donation → Account, RabbitMQ `DonationMadeEvent` | Async, doesn't block payment response |
| Subscription notification | Video → Account, RabbitMQ `UserSubscribedEvent` | Async |
| Recipient validation before donation | Donation → Account, HTTP `GET /api/profile/{id}` | Synchronous correctness check |
| Username resolution on video listings | Video → Account, gRPC `GetUsernames` | High-volume batch lookups |
| Real-time chat | Frontend ↔ Chat, SignalR over WebSocket through the gateway | Push delivery |

All inter-service calls (HTTP and gRPC) are authenticated by a shared `INTERNAL_API_KEY` header (`X-Internal-Api-Key` for HTTP, `x-internal-api-key` for gRPC).

## Shared message contracts

Event class definitions used by MassTransit producers/consumers live alongside the publishing service in `Messages/` folders (e.g. [services/Glense.AccountService/Messages](../services/Glense.AccountService/Messages/)). Consumers reference the same DTOs via project references.

| Event | Publisher | Consumer(s) |
|-------|-----------|-------------|
| `UserRegisteredEvent` | Account | Donation |
| `DonationMadeEvent` | Donation | Account |
| `UserSubscribedEvent` | Video | Account |

## Why microservices

- **Independent deployability** — services are restarted individually (`kubectl rollout restart deployment/<name>`).
- **Tech / scale isolation** — donation and video have very different load profiles and storage needs (decimal money vs. binary files).
- **Failure isolation** — a Donation outage does not prevent users from registering; wallet creation falls back to a deferred message on the queue.

## Why a gateway

- Single CORS allow-list and origin in the frontend.
- Single JWT trust boundary visible to clients.
- Per-service URLs / ports stay an internal detail.
