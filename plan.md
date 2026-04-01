# Integration Test Plan for Glense Microservices

## PROJECT OVERVIEW

**Glense** is a YouTube-like video platform built as a .NET 8 microservices architecture. It consists of 4 backend microservices + 1 API Gateway + 1 React frontend. All backend services target `net8.0`.

---

## ARCHITECTURE

| Component | Project Path | Port | Database | Role |
|---|---|---|---|---|
| **API Gateway** | `Glense.Server/` | 5050 | None | YARP reverse proxy routing to all services |
| **Account Service** | `services/Glense.AccountService/` | 5000 (REST), 5001 (gRPC) | PostgreSQL `glense_account` | Auth, profiles, notifications |
| **Video Catalogue** | `services/Glense.VideoCatalogue/` | 5002 | PostgreSQL `glense_video` | Videos, playlists, subscriptions, likes, comments |
| **Donation Service** | `Glense.Server/DonationService/` | 5100 | PostgreSQL `glense_donation` | Wallets, donations |
| **Chat Service** | `services/Glense.ChatService/` | 5004 | PostgreSQL `glense_chat` | Chats, messages, SignalR hub |

**Inter-service communication:**
- **RabbitMQ** via MassTransit for async events
- **gRPC** from VideoCatalogue → AccountService (username resolution)
- **HTTP** from DonationService → AccountService (profile lookup for username validation)

---

## SERVICE 1: ACCOUNT SERVICE (`services/Glense.AccountService/`)

**Csproj:** `Glense.AccountService.csproj` — packages: `BCrypt.Net-Next`, `MassTransit.RabbitMQ`, `Grpc.AspNetCore`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.InMemory`, JWT Bearer auth.

**DbContext:** `AccountDbContext` (namespace `Glense.AccountService.Data`)
- `DbSet<User> Users`
- `DbSet<Notification> Notifications`
- Supports InMemory fallback when no connection string is set.

**Models (namespace `Glense.AccountService.Models`):**
- `User` — table `users`: `Id` (Guid), `Username`, `PasswordHash`, `Email`, `ProfilePictureUrl`, `AccountType` (default "user"), `CreatedAt`, `IsActive`, `IsVerified`
- `Notification` — table `notifications`: `Id` (Guid), `UserId` (FK→User), `Title`, `Message`, `Type`, `IsRead`, `RelatedEntityId` (nullable Guid), `CreatedAt`

**DTOs (namespace `Glense.AccountService.DTOs`):**
- `RegisterDto` — `Username`, `Email`, `Password`, `ConfirmPassword`, `AccountType`, `ProfilePictureUrl`
- `LoginDto` — `UsernameOrEmail`, `Password`
- `AuthResponseDto` — `Token`, `User` (UserDto), `ExpiresAt`
- `UserDto` — `Id`, `Username`, `Email`, `ProfilePictureUrl`, `AccountType`, `CreatedAt`, `IsVerified`
- `UpdateProfileDto` — `Username?`, `Email?`, `ProfilePictureUrl?`
- `NotificationDto` — `Id`, `Title`, `Message`, `Type`, `IsRead`, `RelatedEntityId?`, `CreatedAt`
- `CreateNotificationRequest` — record with `UserId`, `Title`, `Message`, `Type`, `RelatedEntityId?`

**Controllers:**
1. `AuthController` (`api/auth`):
   - `POST /api/auth/register` — `RegisterDto` → `AuthResponseDto` or 400
   - `POST /api/auth/login` — `LoginDto` → `AuthResponseDto` or 401

2. `ProfileController` (`api/profile`):
   - `GET /api/profile/search?q=&limit=` — public, searches active users
   - `GET /api/profile/me` — `[Authorize]`, returns current user's `UserDto`
   - `GET /api/profile/{userId:guid}` — public, returns `UserDto` or 404
   - `PUT /api/profile/me` — `[Authorize]`, `UpdateProfileDto` → `UserDto`
   - `DELETE /api/profile/me` — `[Authorize]`, soft-deletes (sets `IsActive=false`)

3. `NotificationController` (`api/notification`):
   - `GET /api/notification?isRead=&skip=&take=` — `[Authorize]`, paginated
   - `GET /api/notification/unread-count` — `[Authorize]`
   - `PUT /api/notification/{notificationId}/read` — `[Authorize]`
   - `PUT /api/notification/read-all` — `[Authorize]`

4. `InternalController` (`api/internal`):
   - `POST /api/internal/notifications` — `[Authorize]`, `CreateNotificationRequest` → `NotificationDto`

**Services (namespace `Glense.AccountService.Services`):**
- `IAuthService` / `AuthService` — `RegisterAsync(RegisterDto)`, `LoginAsync(LoginDto)`, `GenerateJwtToken(...)`. Registration publishes `UserRegisteredEvent` via MassTransit.
- `INotificationService` / `NotificationService` — `CreateNotificationAsync(...)`, `GetUserNotificationsAsync(...)`, `MarkAsReadAsync(...)`, `MarkAllAsReadAsync(...)`, `GetUnreadCountAsync(...)`
- `InternalApiKeyInterceptor` — gRPC server interceptor validating `x-internal-api-key` header.

**gRPC (namespace `Glense.AccountService.GrpcServices`):**
- `AccountGrpcService` — implements `AccountGrpc.AccountGrpcBase`
  - `GetUsername(GetUsernameRequest)` → `GetUsernameResponse`
  - `GetUsernames(GetUsernamesRequest)` → `GetUsernamesResponse`
- Proto file: `Protos/account.proto`, `csharp_namespace = "Glense.AccountService.Protos"`

**MassTransit Consumers (namespace `Glense.AccountService.Consumers`):**
- `DonationMadeEventConsumer` — consumes `DonationMadeEvent`, creates notification for recipient
- `UserSubscribedEventConsumer` — consumes `UserSubscribedEvent`, creates notification for channel owner

**Shared Messages (namespace `Glense.Shared.Messages`):**
- `UserRegisteredEvent` — `UserId`, `Username`, `Email`
- `DonationMadeEvent` — `DonorId`, `RecipientId`, `Amount`, `DonorUsername`
- `UserSubscribedEvent` — `SubscriberId`, `ChannelOwnerId`, `SubscriberUsername`

---

## SERVICE 2: VIDEO CATALOGUE (`services/Glense.VideoCatalogue/`)

**Csproj:** `Glense.VideoCatalogue.csproj` — packages: `MassTransit.RabbitMQ`, `Grpc.Net.ClientFactory`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.InMemory`, JWT Bearer auth.

**DbContext:** `VideoCatalogueDbContext` (namespace `Glense.VideoCatalogue.Data`)
- `DbSet<Videos> Videos`, `DbSet<Playlists> Playlists`, `DbSet<PlaylistVideos> PlaylistVideos`, `DbSet<Subscriptions> Subscriptions`, `DbSet<VideoLikes> VideoLikes`, `DbSet<Comment> Comments`, `DbSet<CommentLike> CommentLikes`
- Supports InMemory fallback.

**Models (namespace `Glense.VideoCatalogue.Models`):**
- `Videos` — `Id`, `Title`, `Description`, `UploadDate`, `UploaderId` (Guid), `ThumbnailUrl`, `VideoUrl`, `ViewCount`, `LikeCount`, `DislikeCount`, `Category`
- `Playlists` — `Id`, `Name`, `Description`, `CreatorId`, `CreationDate`
- `PlaylistVideos` — composite key `(PlaylistId, VideoId)`
- `Subscriptions` — composite key `(SubscriberId, SubscribedToId)`, `SubscriptionDate`
- `VideoLikes` — composite key `(UserId, VideoId)`, `IsLiked`
- `Comment` — `Id`, `VideoId`, `UserId`, `Username`, `Content`, `LikeCount`, `DislikeCount`, `CreatedAt`
- `CommentLike` — composite key `(UserId, CommentId)`, `IsLiked`

**DTOs (namespace `Glense.VideoCatalogue.DTOs`):**
- `UploadRequestDTO` — `IFormFile File`, `Title?`, `Description?`, `IFormFile Thumbnail?`, `Category?`
- `UploadResponseDTO` — `Id`, `Title`, `Description`, `VideoUrl`, `ThumbnailUrl`, `UploadDate`, `UploaderId`, `UploaderUsername`, `ViewCount`, `LikeCount`, `DislikeCount`, `Category`
- `UpdateCategoryDTO` — `Category?`
- `LikeRequestDTO` — `VideoId`, `IsLiked`
- `LikeResponseDTO` — `VideoId`, `IsLiked`, `LikeCount`, `DislikeCount`
- `SubscribeRequestDTO` — `SubscribedToId`
- `SubscribeResponseDTO` — `SubscriberId`, `SubscribedToId`, `SubscriptionDate`
- `CreatePlaylistRequestDTO` — `Name`, `Description?`
- `CreatePlaylistResponseDTO` — `Id`, `Name`, `Description`, `CreationDate`, `CreatorId`
- `PlaylistResponseDTO` — `Id`, `Name`, `Description`, `CreationDate`, `CreatorId`
- `AddPlaylistVideoRequestDTO` — `PlaylistId`, `VideoId`
- `CommentResponseDTO` — `Id`, `VideoId`, `UserId`, `Username`, `Content`, `LikeCount`, `DislikeCount`, `CreatedAt`
- `CreateCommentRequestDTO` — `Content`
- `CommentLikeRequestDTO` — `IsLiked`

**Controllers:**
1. `VideosController` (`api/videos`):
   - `POST /api/videos/upload` — `[Authorize]`, multipart form (file + metadata)
   - `GET /api/videos/search?q=&category=` — public
   - `GET /api/videos` — public, lists all videos
   - `GET /api/videos/{id:guid}` — public, single video detail
   - `PATCH /api/videos/{id:guid}/view` — public, increments view (30min IP-based cache)
   - `PATCH /api/videos/{id:guid}/category` — `[Authorize]`, owner only
   - `GET /api/videos/{id:guid}/thumbnail` — public, serves thumbnail file
   - `GET /api/videos/{id:guid}/stream` — public, serves video file with range support

2. `VideoLikesController` (`api/videolikes`):
   - `GET /api/videolikes/{videoId:guid}` — `[Authorize]`, get user's like status
   - `POST /api/videolikes` — `[Authorize]`, `LikeRequestDTO`

3. `SubscriptionsController` (`api/subscriptions`):
   - `POST /api/subscriptions` — `[Authorize]`, `SubscribeRequestDTO`. Publishes `UserSubscribedEvent`.
   - `DELETE /api/subscriptions` — `[Authorize]`, `SubscribeRequestDTO`

4. `PlaylistsController` (`api/playlists`):
   - `POST /api/playlists` — `[Authorize]`, `CreatePlaylistRequestDTO`
   - `GET /api/playlists` — list (optional `X-Creator-Id` header filter)
   - `GET /api/playlists/{id:guid}` — single playlist

5. `PlaylistVideosController` (`api/playlistvideos`):
   - `POST /api/playlistvideos` — `[Authorize]`, `AddPlaylistVideoRequestDTO`, owner only
   - `DELETE /api/playlistvideos` — `[Authorize]`, owner only
   - `GET /api/playlistvideos/{playlistId:guid}` — list videos in playlist

6. `CommentsController` (`api/videos/{videoId:guid}/comments`):
   - `GET /api/videos/{videoId}/comments` — public
   - `POST /api/videos/{videoId}/comments` — `[Authorize]`, `CreateCommentRequestDTO`
   - `POST /api/videos/{videoId}/comments/{commentId}/like` — `[Authorize]`, `CommentLikeRequestDTO`
   - `DELETE /api/videos/{videoId}/comments/{commentId}` — `[Authorize]`, owner only

**Services (namespace `Glense.VideoCatalogue.Services`):**
- `Upload` — `UploadFileAsync(IFormFile, title, description, uploaderId, thumbnail, category)`
- `IVideoStorage` / `LocalFileVideoStorage` — file I/O abstraction
- `CreatePlaylist` — `CreateAsync(name, description, creatorId)`
- `LikeVideo` — `SetLikeAsync(userId, videoId, isLiked)`
- `Subscribe` — `SubscribeAsync(subscriberId, subscribedToId)`, `UnsubscribeAsync(...)`

**gRPC Client (namespace `Glense.VideoCatalogue.GrpcClients`):**
- `IAccountGrpcClient` / `AccountGrpcClient` — `GetUsernameAsync(Guid)`, `GetUsernamesAsync(IEnumerable<Guid>)` — calls AccountService's gRPC endpoint
- `InternalApiKeyClientInterceptor` — attaches `x-internal-api-key` header to outgoing gRPC calls

---

## SERVICE 3: DONATION SERVICE (`Glense.Server/DonationService/`)

**Csproj:** `DonationService.csproj` — packages: `MassTransit.RabbitMQ`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.InMemory` (EF 9.0), JWT Bearer auth. **Note:** `Program.cs` already exposes `public partial class Program { }` for `WebApplicationFactory` integration tests.

**DbContext:** `DonationDbContext` (namespace `DonationService.Data`)
- `DbSet<Wallet> Wallets`, `DbSet<Donation> Donations`
- Supports InMemory fallback.

**Entities (namespace `DonationService.Entities`):**
- `Wallet` — `Id`, `UserId`, `Balance` (decimal 12,2), `CreatedAt`, `UpdatedAt`
- `Donation` — `Id`, `DonorUserId`, `RecipientUserId`, `Amount` (decimal 12,2), `Message?`, `CreatedAt`

**DTOs (namespace `DonationService.DTOs`):**
- `CreateDonationRequest(DonorUserId, RecipientUserId, Amount, Message?)`
- `DonationResponse(Id, DonorUserId, RecipientUserId, Amount, Message?, CreatedAt)`
- `CreateWalletRequest(UserId, InitialBalance = 0)`
- `TopUpWalletRequest(Amount)`
- `WithdrawWalletRequest(Amount)`
- `WalletResponse(Id, UserId, Balance, CreatedAt, UpdatedAt)`

**Controllers:**
1. `DonationController` (`api/donation`) — all `[Authorize]`:
   - `GET /api/donation/donor/{userId:guid}` — donations by donor
   - `GET /api/donation/recipient/{userId:guid}` — donations by recipient
   - `POST /api/donation` — `CreateDonationRequest`, validates wallets exist, checks balance, transfers funds, publishes `DonationMadeEvent`

2. `WalletController` (`api/wallet`) — all `[Authorize]`:
   - `GET /api/wallet/user/{userId:guid}` — get wallet
   - `POST /api/wallet` — `CreateWalletRequest`, creates or returns existing
   - `POST /api/wallet/user/{userId:guid}/topup` — `TopUpWalletRequest`
   - `POST /api/wallet/user/{userId:guid}/withdraw` — `WithdrawWalletRequest`

**Services (namespace `DonationService.Services`):**
- `IAccountServiceClient` / `AccountServiceClient` — HTTP calls to Account Service's `GET /api/profile/{userId}` to resolve usernames

**MassTransit Consumer (namespace `DonationService.Consumers`):**
- `UserRegisteredEventConsumer` — consumes `UserRegisteredEvent`, creates wallet for new user

**Existing Tests:** `Glense.Server/DonationService/Tests/` contains:
- `DonationService.Tests.csproj` — xUnit, references `DonationService.csproj`, uses `Microsoft.EntityFrameworkCore.InMemory`
- `DonationDbTests.cs` — 2 basic unit tests: `GetAllWallets_ReturnsEmptyList`, `GetAllDonations_ReturnsEmptyList`

---

## SERVICE 4: CHAT SERVICE (`services/Glense.ChatService/`)

**Csproj:** `Glense.ChatService.csproj` — packages: `Npgsql.EntityFrameworkCore.PostgreSQL`, `Microsoft.EntityFrameworkCore.InMemory`, JWT Bearer auth, `DotNetEnv`, `Hellang.Middleware.ProblemDetails`. **No MassTransit** — standalone service.

**DbContext:** `ChatDbContext` (namespace `Glense.ChatService.Data`)
- `DbSet<Chat> Chats`, `DbSet<Message> Messages`
- Uses `IEntityTypeConfiguration` in `Data/Configurations/` (`ChatConfig`, `MessageConfig`)
- Supports InMemory fallback.

**Models (namespace `Glense.ChatService.Models`):**
- `Chat` — `Id`, `Topic`, `CreatedAtUtc`, `Messages` (collection)
- `Message` — `Id`, `ChatId` (FK→Chat), `UserId`, `Username`, `Sender` (enum `MessageSender`), `Content`, `CreatedAtUtc`
- `MessageSender` — enum: `User = 0`, `System = 1`

**DTOs (namespace `Glense.ChatService.DTOs`):**
- `ChatDto(Id, Topic, CreatedAtUtc, MessagesCount)` — record
- `MessageDto(Id, ChatId, UserId, Username, Sender, Content, CreatedAtUtc)` — record
- `CreateChatRequest` — `Topic`
- `CreateMessageRequest` — `Sender` (regex "user|system"), `Content`
- `PagedResponse<T>(Items, NextCursor)` — cursor-based pagination

**Controllers:**
1. `ChatsController` (`api/chats`) — all `[Authorize]`:
   - `GET /api/chats?cursor=&pageSize=` — paginated list
   - `POST /api/chats` — `CreateChatRequest`
   - `GET /api/chats/{chatId:guid}` — single chat
   - `DELETE /api/chats/{chatId:guid}` — delete chat

2. `MessagesController` (`api/chats/{chatId:guid}/messages`) — all `[Authorize]`:
   - `GET /api/chats/{chatId}/messages?cursor=&pageSize=` — paginated list
   - `POST /api/chats/{chatId}/messages` — `CreateMessageRequest`, extracts `userId` and `username` from JWT claims

3. `MessageRootController` (`api/messages`) — all `[Authorize]`:
   - `GET /api/messages/{messageId:guid}` — get single message
   - `DELETE /api/messages/{messageId:guid}` — delete message

**Services (namespace `Glense.ChatService.Services`):**
- `IChatService` / `ChatService` — all CRUD operations with cursor-based pagination

**SignalR Hub (namespace `Glense.ChatService.Hubs`):**
- `ChatHub` at `/hubs/chat` — methods: `SendMessageToChat(chatId, user, message)`, `JoinChat(chatId)`, `LeaveChat(chatId)`

---

## API GATEWAY (`Glense.Server/`)

YARP reverse proxy. Routes defined in `appsettings.json`:
- `/api/auth/**`, `/api/profile/**`, `/api/notification/**` → `account-cluster` (port 5001)
- `/api/videos/**`, `/api/subscriptions/**`, `/api/videolikes/**`, `/api/playlists/**`, `/api/playlistvideos/**` → `video-cluster` (port 5002)
- `/api/donation/**`, `/api/wallet/**` → `donation-cluster` (port 5100)
- `/api/chats/**`, `/api/messages/**` → `chat-cluster` (port 5004)
- `/hubs/chat/**` → `chat-cluster` (port 5004)
- Health endpoint: `GET /health`

---

## SHARED EVENT FLOW (MassTransit/RabbitMQ)

1. **User Registration:** AccountService `AuthService.RegisterAsync()` → publishes `UserRegisteredEvent` → DonationService `UserRegisteredEventConsumer` creates wallet
2. **Subscription:** VideoCatalogue `SubscriptionsController.Subscribe()` → publishes `UserSubscribedEvent` → AccountService `UserSubscribedEventConsumer` creates notification
3. **Donation:** DonationService `DonationController.CreateDonation()` → publishes `DonationMadeEvent` → AccountService `DonationMadeEventConsumer` creates notification

---

## AUTHENTICATION

All services use the same JWT configuration:
- Issuer: `GlenseAccountService`
- Audience: `GlenseApp`
- Secret key: from env `JWT_SECRET_KEY` or config `JwtSettings:SecretKey`
- Token claims: `sub` = userId (Guid), `unique_name` = username, `email`, `account_type`, `jti`
- Token expiry: 7 days
- Password hashing: BCrypt (`BCrypt.Net.BCrypt.HashPassword` / `Verify`)

---

## INFRASTRUCTURE

- **Docker Compose** (`docker-compose.yml`): PostgreSQL 16 containers for each service, RabbitMQ 3 with management, all services on `glense_network`
- All services have a `GET /health` endpoint

---

## EXISTING TEST INFRASTRUCTURE

Only `DonationService` has tests:
- Location: `Glense.Server/DonationService/Tests/`
- Framework: xUnit
- Test project: `DonationService.Tests.csproj` references the main `DonationService.csproj`
- Only 2 basic DB unit tests exist
- DonationService's `Program.cs` already has `public partial class Program { }` for `WebApplicationFactory`

---

## TASK: PLAN INTEGRATION TESTS

Using the architecture above, plan integration tests for **each microservice** that:

1. **Use `WebApplicationFactory<Program>`** to spin up each service in-process with an in-memory database (EF Core InMemory provider — already supported by all services).
2. **Mock external dependencies** per service:
   - **AccountService**: Mock `IPublishEndpoint` (MassTransit) to verify event publishing without RabbitMQ. gRPC can be tested in-process.
   - **VideoCatalogue**: Mock `IAccountGrpcClient` to stub username resolution. Mock `IPublishEndpoint`. Mock or use a test `IVideoStorage` implementation (in-memory).
   - **DonationService**: Mock `IAccountServiceClient` (HTTP calls to AccountService). Mock `IPublishEndpoint`.
   - **ChatService**: No external dependencies to mock — pure DB operations.
3. **Generate JWT tokens** in test helpers using the same secret/issuer/audience so `[Authorize]` endpoints work.
4. **Test each controller endpoint** with happy-path and error scenarios.
5. **Test MassTransit consumers** by directly invoking them with a test `ConsumeContext` and verifying DB state changes.
6. **For each service, create a test project** following the pattern already established by `DonationService.Tests/`:
   - `{ServiceName}.IntegrationTests.csproj` with xUnit, `Microsoft.AspNetCore.Mvc.Testing`, `Microsoft.EntityFrameworkCore.InMemory`
   - A custom `WebApplicationFactory` subclass that replaces `DbContext` with InMemory, registers mock services
   - A JWT token helper for generating auth tokens
   - Test classes per controller

**Important constraints:**
- Do NOT require Docker, RabbitMQ, or PostgreSQL to run tests — everything must be in-memory/mocked.
- Each service must be testable independently.
- The `Program.cs` files that don't have `public partial class Program { }` will need it added (AccountService, VideoCatalogue, ChatService).
- Use the exact class names, namespaces, DTOs, and controller routes listed above — do NOT invent new ones.
- For VideoCatalogue, create a simple `InMemoryVideoStorage : IVideoStorage` test double.
