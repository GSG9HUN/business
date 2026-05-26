# Integration Tests

This folder contains tests that exercise multiple application components together.

## Scope

Integration tests may use:

- the real DI service graph
- EF Core and PostgreSQL through Testcontainers
- real repository implementations
- real service orchestration with mocked external edges

They should not require a live Discord server or real Discord messages.

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Integration"
```

## Startup Refactor Coverage

`Service/ProgramIntegrationTests.cs` verifies the startup split:

- `Program.cs` exits cleanly when `.env` is missing
- `BotApplication` exits cleanly when required settings are missing
- `BotServiceProviderFactory` resolves the core services
- the full startup graph resolves against PostgreSQL
- `DatabaseMigrationRunner` applies pending migrations

## Persistence

The persistence tests use `PostgreSqlTestDatabase` and Testcontainers. Docker must be available for those tests to run.
