# End-to-End Flows

Sequence diagrams for the four critical end-to-end flows. Diagrams use Mermaid (rendered natively by GitHub).

## 1. Registration → wallet creation

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant GW as Gateway
    participant AC as Account
    participant MQ as RabbitMQ
    participant DN as Donation
    participant DB as Donation DB

    FE->>GW: POST /api/auth/register {username, email, password}
    GW->>AC: POST /api/auth/register
    AC->>AC: Hash password (BCrypt), insert user, sign JWT
    AC->>MQ: Publish UserRegisteredEvent {UserId, Username, Email}
    AC-->>FE: 200 {token, user}

    Note over MQ,DN: Asynchronous, after the response is returned
    MQ->>DN: UserRegisteredEvent
    DN->>DB: INSERT Wallet (UserId, Balance=0)
```

The HTTP response returns as soon as the user is persisted. Wallet creation happens out-of-band and is retried by RabbitMQ if Donation is down.

## 2. Make a donation

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant GW as Gateway
    participant DN as Donation
    participant AC as Account
    participant MQ as RabbitMQ
    participant DB as Donation DB

    FE->>GW: POST /api/donation {donorId, recipientId, amount, message}
    GW->>DN: POST /api/donation (Authorization: Bearer)
    DN->>AC: GET /api/profile/{recipientId} (X-Internal-Api-Key)
    AC-->>DN: 200 {username}
    DN->>DB: BEGIN
    DN->>DB: UPDATE Wallets SET balance = balance - amount WHERE UserId = donor
    DN->>DB: UPDATE Wallets SET balance = balance + amount WHERE UserId = recipient
    DN->>DB: INSERT Donation
    DN->>DB: COMMIT
    DN->>MQ: Publish DonationMadeEvent {DonorId, RecipientId, Amount, DonorUsername}
    DN-->>FE: 201 {donation}

    Note over MQ,AC: Asynchronous notification
    MQ->>AC: DonationMadeEvent
    AC->>AC: INSERT Notification for recipient
```

Recipient validation is synchronous (HTTP) so a bad recipient returns `400` immediately. The "you received a donation" notification is asynchronous so a slow Account does not slow the payment response.

## 3. Video listing with username resolution

```mermaid
sequenceDiagram
    participant FE as Frontend
    participant GW as Gateway
    participant VC as Video Catalogue
    participant DB as Video DB
    participant AC as Account (gRPC)

    FE->>GW: GET /api/videos
    GW->>VC: GET /api/videos
    VC->>DB: SELECT * FROM Videos
    DB-->>VC: rows[]
    VC->>AC: gRPC GetUsernames([uploaderId1, uploaderId2, ...]) (x-internal-api-key)
    AC-->>VC: {id → username}
    VC->>VC: Merge usernames into response DTOs
    VC-->>FE: 200 [{id, title, uploaderId, uploaderUsername, ...}]
```

If the gRPC call fails, the listing still returns; `uploaderUsername` is `null` for affected rows.

## 4. Real-time chat

```mermaid
sequenceDiagram
    participant A as Client A
    participant B as Client B
    participant GW as Gateway
    participant CH as Chat
    participant DB as Chat DB

    A->>GW: WebSocket /hubs/chat (Bearer token)
    GW->>CH: Upgrade WS
    A->>CH: invoke JoinChat(chatId)
    CH->>CH: Add A.connection to group "chatId"

    B->>GW: WebSocket /hubs/chat
    GW->>CH: Upgrade WS
    B->>CH: invoke JoinChat(chatId)

    A->>GW: POST /api/chats/{chatId}/messages (Bearer)
    GW->>CH: POST /api/chats/{chatId}/messages
    CH->>DB: INSERT Message
    CH-->>A: 201 {message}

    A->>CH: invoke SendMessageToChat(chatId, user, message)
    CH-->>A: ReceiveMessage(chatId, user, message)
    CH-->>B: ReceiveMessage(chatId, user, message)
```

REST handles persistence; the SignalR hub handles fan-out. A typical client posts the message first, then triggers the broadcast.
