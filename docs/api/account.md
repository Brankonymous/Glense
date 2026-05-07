# Account Service API

Base URL (direct): `http://localhost:5001`. Through gateway: `http://localhost:5050` (same paths).

Auth: most endpoints require `Authorization: Bearer <jwt>` from `/api/auth/login` or `/api/auth/register`.

Source: [services/Glense.AccountService/Controllers/](../../services/Glense.AccountService/Controllers).

## Auth

### `POST /api/auth/register`
Create a new user and return a JWT.

Body:
```json
{
  "username": "johndoe",
  "email": "john@example.com",
  "password": "Password123!",
  "confirmPassword": "Password123!",
  "accountType": "user"
}
```
`accountType` is `user`, `creator`, or `admin`.

Response `200`:
```json
{
  "token": "<jwt>",
  "user": { "id": "<guid>", "username": "johndoe", "email": "...", "profilePictureUrl": null, "accountType": "user", "createdAt": "...", "isVerified": false },
  "expiresAt": "2026-05-14T12:00:00Z"
}
```

Errors: `400` if username/email taken or validation fails.

Side effect: publishes `UserRegisteredEvent` on RabbitMQ → Donation creates a wallet (see [flows/01-registration-and-wallet.md](../flows/01-registration-and-wallet.md)).

### `POST /api/auth/login`
Body: `{ "usernameOrEmail": "...", "password": "..." }`. Same response shape as register. `401` on invalid credentials.

## Profile

### `GET /api/profile/search?q=<text>&limit=20`
Public. Case-insensitive substring match against `username` and `email` of active users. Returns up to `limit` (default 20).

### `GET /api/profile/me`
Auth required. Returns the current user's profile.

### `GET /api/profile/{userId:guid}`
Public. Returns active user by ID. `404` if not found or inactive.

### `PUT /api/profile/me`
Auth required. Body (all optional):
```json
{ "username": "...", "email": "...", "profilePictureUrl": "..." }
```
Errors: `400` if new username/email collides with another user. Email change clears `isVerified`.

### `DELETE /api/profile/me`
Auth required. Soft-delete (sets `is_active = false`).

## Notifications

All endpoints require auth.

### `GET /api/notification?isRead=&skip=0&take=20`
Returns the current user's notifications, newest first.

### `GET /api/notification/unread-count`
Returns `{ "count": <int> }`.

### `PUT /api/notification/{notificationId:guid}/read`
Mark one as read.

### `PUT /api/notification/read-all`
Mark all as read for the current user.

## Internal

### `POST /api/internal/notifications`
Used by other services to create notifications when an event-driven path isn't available. Body:
```json
{ "userId": "<guid>", "title": "...", "message": "...", "type": "donation|subscription|comment|system", "relatedEntityId": "<guid?>" }
```

> Note: this controller is decorated `[Authorize]` (JWT). Inter-service notifications are normally raised via RabbitMQ events (`DonationMadeEvent`, `UserSubscribedEvent`); this REST path is the fallback.

## gRPC — `AccountGrpc`

Cleartext HTTP/2 on port `5003` (host) / `5001` (container). Contract: [Protos/account.proto](../../services/Glense.AccountService/Protos/account.proto).

| RPC | Request | Response |
|-----|---------|----------|
| `GetUsername(GetUsernameRequest)` | `string user_id` | `string user_id, string username, bool found` |
| `GetUsernames(GetUsernamesRequest)` | `repeated string user_ids` | `repeated UserMapping users` |

All gRPC calls require the `INTERNAL_API_KEY` header (enforced by [`InternalApiKeyInterceptor`](../../services/Glense.AccountService/GrpcServices)).

## Health

### `GET /health` — `{ status: "healthy", service: "account", timestamp }`

## Database schema

See [services/Glense.AccountService/database/schema.sql](../../services/Glense.AccountService/database/schema.sql). Tables: `users`, `notifications`.
