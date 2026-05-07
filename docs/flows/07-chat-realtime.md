# Flow: Real-time chat

## Trigger

User opens a chat room in the SPA.

## Sequence

```mermaid
sequenceDiagram
    participant FE as Chat.jsx
    participant GW as Gateway
    participant CH as Chat Service
    participant Hub as ChatHub
    participant DB as Postgres (chat)

    FE->>GW: GET /api/chats/{chatId}/messages?cursor=&pageSize=50
    GW->>CH: forward (JWT)
    CH-->>FE: page of messages

    FE->>GW: WebSocket /hubs/chat (Bearer JWT)
    GW->>Hub: upgrade
    Hub-->>FE: connected
    FE->>Hub: invoke JoinChat(chatId)
    Hub->>Hub: Groups.AddToGroupAsync(connectionId, chatId)

    Note over FE: user sends a message

    FE->>GW: POST /api/chats/{chatId}/messages { content }
    GW->>CH: forward
    CH->>CH: extract userId+username from JWT
    CH->>DB: INSERT message
    CH-->>FE: 201 (Location: /api/messages/{id})

    FE->>Hub: invoke SendMessageToChat(chatId, user, message)
    Hub->>Hub: Clients.Group(chatId).SendAsync("ReceiveMessage", ...)
    Hub-->>FE: ReceiveMessage(chatId, user, message) (to all members)
```

## Code refs

- REST: [ChatsController.cs](../../services/Glense.ChatService/Controllers/ChatsController.cs), [MessagesController.cs](../../services/Glense.ChatService/Controllers/MessagesController.cs), [MessageRootController.cs](../../services/Glense.ChatService/Controllers/MessageRootController.cs)
- Hub: [Glense.ChatService/Hubs/ChatHub.cs](../../services/Glense.ChatService/Hubs/ChatHub.cs)
- Service layer: `IChatService` in [services/Glense.ChatService/Services/](../../services/Glense.ChatService/Services)
- Frontend: [glense.client/src/components/Chat/](../../glense.client/src/components/Chat)

## Notes

- The hub is currently broadcast-only; persistence is the REST `POST` above. A future improvement is to persist inside the hub method or to fan out from the controller.
- Cursor pagination uses the message's `Id` (Guid v7-style ordering by `CreatedAtUtc`). Pass the last message's `id` as the next `cursor`.
- Chat hub negotiation must include the JWT — SignalR's `accessTokenFactory` is the standard client-side pattern.

## Related APIs

- [`GET /api/chats`](../api/chat.md)
- [`POST /api/chats/{chatId}/messages`](../api/chat.md)
- WebSocket [`/hubs/chat`](../api/chat.md)
