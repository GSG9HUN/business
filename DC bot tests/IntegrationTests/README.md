# Integration Tests

This folder contains tests that exercise multiple application components together.

## Scope

Integration tests may use:

- the real DI service graph
- EF Core and PostgreSQL through Testcontainers
- real repository implementations
- real service orchestration with mocked external edges
- fake Discord wrapper contexts for command-handler routing tests

They should not require a live Discord server or real Discord messages.

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Integration"
```

## Startup Refactor Coverage

`Service/ProgramIntegrationTests.cs` verifies the startup split:

- `Program.cs` continues when `.env` is missing and relies on environment variables
- `BotApplication` exits cleanly when required settings are missing
- `BotServiceProviderFactory` resolves the core services
- the full startup graph resolves against PostgreSQL
- `DatabaseMigrationRunner` applies pending migrations
- DSharpPlus Commands and `SlashCommandProcessor` resolve with the slash command modules
- the startup graph resolves all 15 text command implementations

## Persistence

The persistence tests use `PostgreSqlTestDatabase` and Testcontainers. Docker must be available for those tests to run.

Covered PostgreSQL-backed areas include:

- queue claim/reorder concurrency and unique-position behavior
- guild premium state
- playback repeat/current-track state
- repeat-list snapshot replace/clear
- `MusicQueueService` enqueue, reorder, dequeue, clear, and repeatable snapshot flow
- `RepeatService` and `CurrentTrackService` shared playback state
- `TrackEndedHandlerService` current-item state transition and repeat-list requeue behavior

## Command Routing

`Service/Core/CommandHandlerServiceIntegrationTests.cs` covers fake Discord message events routed through the real text command list for utility, music, and queue command paths. The command handler uses an injectable `IDiscordMessageFactory` boundary so tests can provide stable Discord wrapper contexts without relying on DSharpPlus internal cache state.

## Localization

`Service/Localization/LocalizationJsonIntegrationTests.cs` reads the real `localization/eng.json` and `localization/hu.json` files and verifies key slash-command fallback strings are present in both languages.
