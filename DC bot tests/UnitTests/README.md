# Unit Tests

This folder contains isolated tests for commands, services, wrappers, configuration, persistence helpers, and models.

## Scope

Unit tests should:

- avoid network calls
- avoid real Discord or Lavalink connections
- use mocks for external dependencies
- verify behavior, logging, error handling, and event contracts

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Unit"
```

## Startup Refactor Coverage

The startup-related unit coverage lives mainly in:

- `Wrapper/DiscordClientFactoryTests.cs`
- `Wrapper/DiscordClientEventHandlerTests.cs`

These tests verify that client creation is independent from event handler construction and that `DiscordClientEventHandler` uses direct dependencies.
