# Testing

All four backend services have integration tests under [tests/](../tests/) using `WebApplicationFactory<Program>` with EF Core InMemory and mocked external dependencies (RabbitMQ, gRPC, HTTP). **No Docker, no Postgres, no RabbitMQ are required to run the suite.**

## Layout

| Project | Tests for |
|---------|-----------|
| [tests/AccountService.IntegrationTests/](../tests/AccountService.IntegrationTests) | Auth, profile, notifications, gRPC service |
| [tests/VideoCatalogue.IntegrationTests/](../tests/VideoCatalogue.IntegrationTests) | Videos, comments, subscriptions, likes, playlists |
| [tests/DonationService.IntegrationTests/](../tests/DonationService.IntegrationTests) | Wallets, donations, recipient-validation flow |
| [tests/ChatService.IntegrationTests/](../tests/ChatService.IntegrationTests) | Chats, messages, JWT enforcement |
| [tests/Glense.TestUtilities/](../tests/Glense.TestUtilities) | Shared helpers — JWT token generation, `WebApplicationFactory` overrides |

The `Program` class is exposed via `public partial class Program { }` at the bottom of each service's `Program.cs` so the factory can host it.

## Run

```bash
# All services
dotnet test

# A single service
dotnet test tests/AccountService.IntegrationTests

# Filter by name
dotnet test --filter "FullyQualifiedName~Auth"
```

PowerShell convenience runner with formatted per-project output:

```powershell
./scripts/run_integration_tests.ps1
./scripts/run_integration_tests.ps1 -Project Account
./scripts/run_integration_tests.ps1 -Filter "FullyQualifiedName~Auth"
```

A Python alternative is [scripts/run_all_tests.py](../scripts/run_all_tests.py).

## Patterns to follow

- **Database isolation per test class.** Override `AddDbContext<T>` in `WebApplicationFactory.ConfigureServices` to register an InMemory provider with a fresh database name (typically a `Guid`).
- **JWT generation.** Use `Glense.TestUtilities` to mint a token with the same `JwtSettings:SecretKey` your test factory configures. Set the `Authorization: Bearer <token>` header on the test `HttpClient`.
- **MassTransit.** Replace the bus with the in-memory test harness (`x.UsingInMemory(...)`) and assert published events with `IPublishedMessage<T>`.
- **gRPC client.** In Video tests, replace `IAccountGrpcClient` with a fake returning canned username maps.
- **HTTP-to-Account.** In Donation tests, replace `IAccountServiceClient` with a fake.

## Adding a new test project

1. `dotnet new xunit -o tests/MyService.IntegrationTests` and add to [Glense.sln](../Glense.sln).
2. Reference the service project and `Glense.TestUtilities`.
3. Inherit from a `WebApplicationFactory<Program>` subclass that swaps DB + bus + external clients to test doubles.
4. Mirror the controller folder structure with one test class per controller.
