# Tests

Integration tests for every backend service, all runnable with `dotnet test` — no Docker, no Postgres, no RabbitMQ needed.

| Project | Covers |
|---------|--------|
| [AccountService.IntegrationTests/](AccountService.IntegrationTests) | Auth, profile, notifications, gRPC `AccountGrpc` |
| [VideoCatalogue.IntegrationTests/](VideoCatalogue.IntegrationTests) | Videos, comments, subscriptions, likes, playlists |
| [DonationService.IntegrationTests/](DonationService.IntegrationTests) | Wallets, donations, recipient validation |
| [ChatService.IntegrationTests/](ChatService.IntegrationTests) | Chats, messages, JWT enforcement |
| [Glense.TestUtilities/](Glense.TestUtilities) | Shared helpers — JWT generation, factory overrides |

See [docs/TESTING.md](../docs/TESTING.md) for run commands, patterns, and how to add a new project.
