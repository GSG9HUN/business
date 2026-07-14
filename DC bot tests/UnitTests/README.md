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

## Production Refactor Coverage

- `Service/Core/CommandRegistryTests.cs` covers command registry lookup, single command-list materialization, and duplicate command names.
- `Service/Core/CommandValidationServiceTests.cs` covers missing and whitespace-only command arguments.
- `Service/Music/MusicQueueServiceTests.cs` verifies `ITrackSerializer` is used for queue persistence boundaries.
- `Service/Music/PlaylistServiceTests.cs` covers saved playlist service result mapping and repository interaction.
- `Commands/TextCommands/Playlist/` contains one test file per playlist command plus a shared test base.
- `Persistence/PlaylistRepositoryTests.cs` and `Persistence/PlaylistTrackRepositoryTests.cs` cover in-memory repository behavior.
- `Persistence/BotDbContextFactoryTests.cs` and `Persistence/PostgreSqlQueueClaimSqlTests.cs` cover design-time connection-string creation and queue claim SQL generation.
- `Service/ReactionHandler/ReactionContextFactoryTests.cs` and `Startup/BotHandlerRegistrarTests.cs` cover reaction context creation and handler registration wiring.
- `Wrapper/SlashInteractionContextFactoryTests.cs` covers slash context wrapper creation.
- `Model/PlaylistModelTests.cs` protects playlist DTO, record, enum, and entity contracts.
- `Service/Music/PlayerConnectionServiceTests.cs` covers cancellation propagation during connection retry waits.
