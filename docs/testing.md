# Testing

Glense uses xUnit integration tests per service. They run entirely in-process — **no Docker, no Postgres, no RabbitMQ required**. EF Core uses the InMemory provider, MassTransit and gRPC clients are replaced with mocks.

## Test projects

| Project | Target service |
|---------|----------------|
| [../tests/AccountService.IntegrationTests](../tests/AccountService.IntegrationTests/) | Account |
| [../tests/VideoCatalogue.IntegrationTests](../tests/VideoCatalogue.IntegrationTests/) | Video Catalogue |
| [../tests/DonationService.IntegrationTests](../tests/DonationService.IntegrationTests/) | Donation |
| [../tests/ChatService.IntegrationTests](../tests/ChatService.IntegrationTests/) | Chat |
| [../tests/Glense.TestUtilities](../tests/Glense.TestUtilities/) | Shared helpers (JWT generation, MassTransit removal, JWT auth override) |

Each project ships a `Custom<Service>Factory : WebApplicationFactory<Program>` that:

1. Sets test JWT settings (`JwtTokenHelper.TestSecretKey`, issuer, audience).
2. Removes the production `DbContext` and registers an InMemory database with a per-test name.
3. Removes MassTransit and registers a `Mock<IPublishEndpoint>` exposed on the factory.
4. Replaces external clients (Account gRPC, video storage) with mocks/in-memory equivalents.
5. Calls `OverrideJwtAuthentication()` so the JWT signing key always matches the test secret.

Example: [../tests/AccountService.IntegrationTests/CustomAccountServiceFactory.cs](../tests/AccountService.IntegrationTests/CustomAccountServiceFactory.cs).

## Running the tests

### All projects

```bash
dotnet test
```

### Pretty PowerShell runner

```powershell
.\scripts\run_integration_tests.ps1
```

Options:

```powershell
.\scripts\run_integration_tests.ps1 -Project Account
.\scripts\run_integration_tests.ps1 -Project Video
.\scripts\run_integration_tests.ps1 -Project Donation
.\scripts\run_integration_tests.ps1 -Project Chat
.\scripts\run_integration_tests.ps1 -Filter "FullyQualifiedName~WalletController"
```

### Single project

```bash
dotnet test tests/AccountService.IntegrationTests
```

### Filter by name

```bash
dotnet test --filter "FullyQualifiedName~Auth"
```

## Test users (full-stack only)

When running the full stack via `python3 scripts/start.py`, [../scripts/seed.py](../scripts/seed.py) creates these users (password `Password123!`):

| Username | Type | Wallet |
|----------|------|--------|
| keki | creator | $500 |
| irena | creator | $500 |
| branko | user | $500 |

These are not used by the integration tests; they exist for manual / E2E testing through the UI and gateway.

## Adding a new test

1. Pick the right `*.IntegrationTests` project.
2. Inject the corresponding `Custom*Factory` via `IClassFixture<Custom*Factory>`.
3. Use `factory.CreateClient()` for HTTP, `JwtTokenHelper.Generate(...)` for an auth header, and `factory.MockPublishEndpoint.Verify(...)` to assert events were published.

See existing tests in any of the four projects for canonical patterns.
