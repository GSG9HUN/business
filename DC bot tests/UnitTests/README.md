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
- `Service/Music/Playlist/` covers saved playlist service result mapping and repository interaction with a shared playlist service test base.
- `Service/Music/PlayerConnection/` covers join, retry, and existing-player behavior with a shared player connection test base.
- `Service/Music/ProgressiveTimer/` covers now-playing timer start/lifecycle behavior through a deterministic fake `IProgressTicker`. Split files: `ProgressiveTimerServiceTestBase.cs`, `ProgressiveTimerServiceStartTests.cs`, `ProgressiveTimerServiceLifecycleTests.cs`.
- `Service/Music/PlaybackControl/` covers pause, resume, skip, and leave behavior through `PlaybackControlServiceTestBase.cs`, `PlaybackControlServicePauseTests.cs`, `PlaybackControlServiceResumeTests.cs`, `PlaybackControlServiceSkipTests.cs`, and `PlaybackControlServiceLeaveTests.cs`.
- `Service/Localization/` covers default translations, guild language persistence, directory creation, formatting fallback, and error handling through `LocalizationServiceTestBase.cs`, `LocalizationServiceDefaultTranslationTests.cs`, `LocalizationServiceGuildLanguageTests.cs`, `LocalizationServiceDirectoryTests.cs`, `LocalizationServiceFormattingTests.cs`, and `LocalizationServiceErrorHandlingTests.cs`.
- `Commands/TextCommands/Music/PlayCommand*.cs` covers metadata, validation, URL playback, and query playback through `PlayCommandTestBase.cs`, `PlayCommandMetadataTests.cs`, `PlayCommandValidationTests.cs`, `PlayCommandUrlPlaybackTests.cs`, and `PlayCommandQueryPlaybackTests.cs`.
- `Commands/TextCommands/Playlist/` contains one test file per playlist command plus a shared test base.
- `Persistence/PlaylistRepositoryTests.cs` and `Persistence/PlaylistTrackRepositoryTests.cs` cover in-memory repository behavior.
- `Persistence/BotDbContextFactoryTests.cs` and `Persistence/PostgreSqlQueueClaimSqlTests.cs` cover design-time connection-string creation and queue claim SQL generation.
- `Service/ReactionHandler/` covers reaction context creation, action dispatch, control message publishing, and `ReactionHandlerService` registration/dispatch/error paths. Split files include `ReactionHandlerServiceTestBase.cs`, `ReactionHandlerServiceRegistrationTests.cs`, `ReactionHandlerServiceDispatchTests.cs`, `ReactionHandlerServiceExceptionLoggingTests.cs`, `ReactionHandlerServiceControlMessageTests.cs`, `ReactionActionDispatcherTests.cs`, `ReactionContextFactoryTests.cs`, `ReactionContextTests.cs`, `ReactionControlMessageServiceTests.cs`, and `ReactionControlEmojisTests.cs`.
- `Wrapper/SlashInteractionContextFactoryTests.cs` covers slash context wrapper creation.
- `Model/PlaylistModelTests.cs` protects playlist DTO, record, enum, and entity contracts.
