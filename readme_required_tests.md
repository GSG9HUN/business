# Required Tests - DC Bot

This file tracks which test areas are already covered and which test areas are still required. It is a living checklist, not only a command reference.

## Executive Summary

**Current automated test inventory from the latest local run:**

- Non-E2E executed tests: **658 passed**
- Targeted live guild/music E2E additions: **5 passed** against real Discord, PostgreSQL Testcontainers, and local Lavalink (`LAVALINK_HOSTNAME=127.0.0.1`).
- Targeted live play/pause/resume/skip/leave music-flow E2E: **1 passed** against real Discord voice and local Lavalink.
- Slash command targeted E2E: **16 passed**
- Latest full E2E suite attempt before the focused additions: **68 passed, 1 failed** because Discord returned `RateLimitException: TooManyRequests` during an existing reaction-flow Discord client connect.

The source-method count and the `dotnet test` result count can differ because xUnit `[Theory]` tests can produce multiple executed test cases from one method.

**Current status:**

- COMPLETE: Core text commands, music services, persistence, startup composition, wrappers, validation, localization, logging/error paths.
- COMPLETE: PostgreSQL persistence integration coverage through Testcontainers.
- COMPLETE: Targeted integration coverage now includes text command DI, command-handler routing through the real text command list, direct PostgreSQL repositories, queue/repeat/current-track services, track-ended persistence orchestration, and real localization JSON loading.
- COMPLETE FOR CURRENT AUTOMATED SCOPE: E2E coverage exists for Discord lifecycle, command messages, reaction handling, wrappers, guild initialization, and live music flows.
- COMPLETE: Slash command unit, integration, and E2E-category pipeline coverage.
- COMPLETE: BotApplication E2E startup coverage with real Discord, Lavalink settings, and PostgreSQL Testcontainers.
- CONFIG-GATED: Real Lavalink music-flow E2E coverage exists for play/pause/resume/skip/leave, track-end auto-advance, repeat, repeat-list, and reaction controls when `DISCORD_TEST_VOICE_CHANNEL_ID` is configured.
- COMPLETE: Live Discord slash invocation manual smoke checklist is documented.

**Required normal verification:**

```bash
dotnet restore "DC bot.sln"
dotnet build "DC bot.sln" --configuration Debug --no-restore
dotnet test "DC bot tests/DC bot tests.csproj" --configuration Debug --no-build --filter "Category!=E2E"
```

---

## Required Unit Tests

### BotService - COMPLETE

- [x] `StartAsync` connects the Discord client when called.
- [x] `StartAsync` logs and throws when `ConnectAsync` fails.
- [x] `StartAsync` waits indefinitely outside test mode.
- [x] `StartAsync` returns immediately in test mode.
- [x] Additional lifecycle/error-path coverage in `UnitTests/Service/BotServiceTests.cs`.

### DiscordClientFactory - COMPLETE

- [x] Creates `DiscordClient` from `BotSettings`.
- [x] Applies expected Discord intents.
- [x] Throws when token is missing.
- [x] Keeps event subscription outside the factory.

### DiscordClientEventHandler - COMPLETE

- [x] `OnClientReady` logs ready state.
- [x] `OnGuildAvailable` initializes localization, Lavalink, queue, and guild data dependencies.
- [x] Handles startup dependency calls without rethrowing unexpected null-event paths.
- [x] Covered by both unit and integration-style tests.

### ReactionHandler - COMPLETE

- [x] Registers reaction event handlers.
- [x] Logs first registration.
- [x] Handles duplicate registration.
- [x] Unregisters after registration.
- [x] Logs warning when unregister is called before register.
- [x] Covers test-mode reaction add/remove behavior.

### LocalizationService - COMPLETE

- [x] Loads default language when guild language file is missing.
- [x] Throws `LocalizationException` when JSON loading fails in production mode.
- [x] Handles JSON deserialization failure paths.
- [x] Returns the key when translation is missing.
- [x] Persists and resolves guild language values.

### Configuration Models - COMPLETE

- [x] `BotSettings`
- [x] `LavalinkSettings`
- [x] `SearchResolverOptions`
- [x] `BotConfigurationLoader` - validates required environment values, trims quoted values, applies defaults, and builds PostgreSQL connection strings.

### Exceptions - COMPLETE

- [x] `BotException`
- [x] `LocalizationException`
- [x] `MessageSendException`
- [x] `LavalinkOperationException`
- [x] `QueueOperationException`
- [x] `TrackLoadException`
- [x] `ValidationException`

### SlashCommands - COMPLETE

Slash commands are tested through the framework-facing modules and the shared `ISlashCommandExecutor` pipeline that delegates to the existing text commands.

- [x] `PlaySlashCommand` - defers response when invoked.
- [x] `PlaySlashCommand` - validates user voice channel.
- [x] `PlaySlashCommand` - handles URL vs query input.
- [x] `PlaySlashCommand` - responds with success or validation error.
- [x] `JoinSlashCommand` - delegates to `JoinCommand` and starts queued playback through the shared pipeline.
- [x] `TagSlashCommand` - exposes a Discord member option as `/tag user:<member>`.
- [x] `TagSlashCommand.Tag` - wraps the DSharpPlus context and forwards the selected member mention.
- [x] `TagSlashCommand.ExecuteAsync` - forwards `IDiscordMember.Mention` to the executor.
- [x] `TagSlashCommand` - tags a member successfully through the shared pipeline.
- [x] `PingSlashCommand` - responds with `Pong!`.
- [x] `HelpSlashCommand` - lists available commands.
- [x] `QueueSlashCommand` - delegates to `ViewQueueCommand`.
- [x] `ShuffleSlashCommand` - delegates to `ShuffleCommand`.
- [x] `RepeatSlashCommand` - delegates `track` and `list` subcommands to the matching queue commands.
- [x] `LanguageSlashCommand` - maps `eng` and `hu` choices to text command arguments.
- [x] `LanguageSlashCommand` - rejects unsupported enum values without executing the command.
- [x] `ClearSlashCommand` - requires `confirm:true` before delegating to `ClearCommand`.
- [x] Slash command module entrypoints wrap DSharpPlus context and delegate to the executor.
- [x] `SlashCommandExecutor` - handles guild-only, missing-command, deferred fallback, domain exception, and unexpected exception paths.

### Commands - Utility - COMPLETE

- [x] `PingCommand` - responds with Pong.
- [x] `PingCommand` - ignores bot users.
- [x] `PingCommand` - name and description.
- [x] `HelpCommand` - lists commands.
- [x] `HelpCommand` - handles empty command list.
- [x] `HelpCommand` - ignores bot users.
- [x] `TagCommand` - tags user successfully.
- [x] `TagCommand` - handles missing arguments.
- [x] `TagCommand` - handles not-found users.
- [x] `TagCommand` - handles case-insensitive and whitespace input.
- [x] `TagCommand` - handles Discord member mentions from slash command input.
- [x] `TagCommand` - handles null guild/member edge cases.
- [x] `LanguageCommand` - returns usage when language is missing.
- [x] `LanguageCommand` - validates language code.
- [x] `LanguageCommand` - normalizes language code.
- [x] `LanguageCommand` - handles save failure.

### Commands - Music - COMPLETE

- [x] `JoinCommand` - joins voice channel.
- [x] `JoinCommand` - handles validation errors.
- [x] `JoinCommand` - ignores bot users.
- [x] `LeaveCommand` - leaves voice channel.
- [x] `LeaveCommand` - handles no active connection.
- [x] `LeaveCommand` - ignores bot users.
- [x] `PauseCommand` - pauses playback.
- [x] `PauseCommand` - handles validation errors.
- [x] `ResumeCommand` - resumes playback.
- [x] `ResumeCommand` - handles validation errors.
- [x] `SkipCommand` - skips playback.
- [x] `SkipCommand` - handles validation errors.
- [x] `PlayCommand` - handles missing query/URL.
- [x] `PlayCommand` - validates voice channel.
- [x] `PlayCommand` - plays URL inputs.
- [x] `PlayCommand` - plays search-query inputs.
- [x] `PlayCommand` - covers YouTube, YouTube Music, Spotify, SoundCloud, Apple Music, Deezer, and Yandex Music modes.
- [x] `PlayCommand` - ignores bot users.

### Commands - Queue - COMPLETE

- [x] `ClearCommand` - clears queue.
- [x] `ClearCommand` - handles validation errors.
- [x] `RepeatCommand` - toggles repeat mode.
- [x] `RepeatCommand` - handles repeat-list conflict.
- [x] `RepeatCommand` - handles validation errors.
- [x] `RepeatListCommand` - toggles repeat-list mode.
- [x] `RepeatListCommand` - handles repeat-track conflict.
- [x] `RepeatListCommand` - handles current-track-null repeat-list snapshot.
- [x] `ShuffleCommand` - handles empty queue.
- [x] `ShuffleCommand` - handles single-track queue.
- [x] `ShuffleCommand` - shuffles valid queues.
- [x] `ShuffleCommand` - covers retry path for identical references.
- [x] `ViewQueueCommand` - handles empty queue.
- [x] `ViewQueueCommand` - displays queue items.
- [x] `ViewQueueCommand` - displays footer when queue has more than 10 tracks.

### Core Services - COMPLETE

- [x] `CommandHandlerService` - register/unregister handler.
- [x] `CommandHandlerService` - duplicate registration.
- [x] `CommandHandlerService` - unknown command.
- [x] `CommandHandlerService` - prefix validation.
- [x] `CommandHandlerService` - command dispatch and logging.
- [x] `CommandValidationService` - argument parsing and validation helpers.
- [x] `ValidationService` - user, player, and connection validation.
- [x] `ResponseBuilder` - message and embed response behavior.

### Music Services - COMPLETE

- [x] `LavaLinkService` facade delegates to focused services.
- [x] `LavalinkNodeConnectionService` handles idempotent and concurrent connect.
- [x] `LavalinkNodeConnectionService` maps startup failures to domain exception.
- [x] `PlaybackRequestService` handles URL and query loading.
- [x] `PlaybackRequestService` handles track-not-found and load exceptions.
- [x] `PlaybackControlService` handles pause, resume, skip, leave, and error paths.
- [x] `PlayerConnectionService` handles join, existing player validation, retry, and exception paths.
- [x] `MusicQueueService` handles enqueue/dequeue/view/get/set/clear.
- [x] `MusicQueueService` handles invalid stored track identifiers.
- [x] `MusicQueueService` handles repeatable queue snapshots.
- [x] `RepeatService` handles repeat and repeat-list flags.
- [x] `CurrentTrackService` stores/restores current track and queue item ID.
- [x] `TrackEndedHandlerService` handles repeat, repeat-list, normal queue advance, and empty queue.
- [x] `TrackPlaybackService` handles immediate play and queue behavior.
- [x] `TrackFormatterService` formats current and queued track output.
- [x] `TrackNotificationService` sends now-playing notifications.
- [x] `PlaybackEventHandlerService` registers and cleans up playback event handlers.
- [x] `ProgressiveTimerService` covers timer start, stop, cancellation, position bounds, and message modification failures.
- [x] `TrackSearchResolverService` covers URL/query source resolution and default/fallback behavior.

### Persistence - COMPLETE

- [x] `GuildDataRepository`
- [x] `PlaybackStateRepository`
- [x] `QueueRepository`
- [x] `RepeatListRepository`
- [x] `GuildPremiumAuditEntity`

### IO, Models, Helpers, Wrappers - COMPLETE

- [x] `PhysicalFileSystem`
- [x] `SerializedTrack`
- [x] validation result models
- [x] `DiscordChannelWrapper`
- [x] `DiscordGuildWrapper`
- [x] `DiscordMemberWrapper`
- [x] `DiscordMessageWrapper`
- [x] `SlashInteractionMessageWrapper`
- [x] `DiscordUserWrapper`
- [x] `DiscordVoiceStateWrapper`
- [x] `LavalinkTrackWrapper`

---

## Required Integration Tests

### Startup and DI - COMPLETE

- [x] `Program.Main` loads `.env` only when present and falls through to environment validation.
- [x] `BotApplication.RunAsync` reports missing `DISCORD_TOKEN`.
- [x] `BotApplication.RunAsync` reports missing `LAVALINK_HOSTNAME`.
- [x] `BotServiceProviderFactory` registers core services.
- [x] Startup graph resolves all 15 text command implementations.
- [x] Full startup graph resolves against PostgreSQL.
- [x] `DatabaseMigrationRunner` rejects in-memory migration provider.
- [x] `DatabaseMigrationRunner` applies pending PostgreSQL migrations.

### PostgreSQL Persistence - COMPLETE

- [x] Playback queue and repeat state survive repository recreation.
- [x] `GuildDataRepository` persists premium state and expiry behavior.
- [x] `PlaybackStateRepository` persists repeat flags, current track identifier, and queue item ID.
- [x] `RepeatListRepository` replaces, orders, and clears repeat-list snapshots.
- [x] `QueueRepository.ClaimNextQueuedItemAsync` claims the lowest queued item.
- [x] Concurrent claim does not claim the same queued item twice.
- [x] Reorder works against PostgreSQL unique position index without constraint collision.

### CommandHandlerService Integration - COMPLETE

- [x] Register/unregister behavior.
- [x] Unknown command response.
- [x] DI command resolution.
- [x] Prefix validation and logging.
- [x] Fake Discord message events route through the real text command list for `ping`, `help`, `language`, `tag`, one music command, and one queue command.

### SlashCommand Integration - COMPLETE

- [x] Startup graph resolves `ISlashCommandExecutor`.
- [x] Startup graph resolves `ISlashInteractionContextFactory`.
- [x] Startup graph resolves slash command modules.
- [x] DSharpPlus Commands extension resolves from DI with `SlashCommandProcessor`.
- [x] Startup graph resolves `TagSlashCommand` with a Discord member option.
- [x] Executor from startup graph executes the slash ping pipeline.

### LavaLinkService Integration - COMPLETE

- [x] `Init` initializes current track and repeat state.
- [x] `ConnectAsync` is idempotent.
- [x] `ConnectAsync` maps startup failure to `LavalinkOperationException`.
- [x] `StartPlayingQueue` plays queued track and updates current track.
- [x] `StartPlayingQueue` handles empty queue.
- [x] `LeaveVoiceChannel` stops, cleans up, and disconnects.

### TrackFormatterService Integration - COMPLETE

- [x] Formats track metadata.
- [x] Handles missing track info gracefully.

### Music Service Persistence Integration - COMPLETE

- [x] `MusicQueueService` enqueues, bulk-enqueues, reorders, dequeues, clears, and reads queue state through real PostgreSQL repositories.
- [x] `MusicQueueService.GetRepeatableQueue` reads repeat-list snapshots through the real repeat-list repository.
- [x] `RepeatService` and `CurrentTrackService` share persisted playback state through real repositories.
- [x] `TrackEndedHandlerService` marks the current queue item played and starts the next queued track with real persistence and mocked playback.
- [x] `TrackEndedHandlerService` requeues repeat-list snapshots with real persistence and mocked playback.

### Localization Integration - COMPLETE

- [x] Real `localization/eng.json` and `localization/hu.json` files load successfully.
- [x] Important slash fallback texts exist in both English and Hungarian.

---

## Required E2E Tests

E2E tests require real Discord configuration and are excluded from normal PR verification. The E2E workflow starts PostgreSQL and Lavalink containers, mounts `lavalink-server/application.yaml`, and runs `Category=E2E` by default.

Required E2E settings:

- `DISCORD_TOKEN`
- `DISCORD_TEST_GUILD_ID`
- `DISCORD_TEST_CHANNEL_ID`
- `DISCORD_TEST_VOICE_CHANNEL_ID`
- `LAVALINK_HOSTNAME`
  - Local host run against `docker-compose.yaml`: `127.0.0.1`
  - CI/container run inside the Docker network: `lavalink`
- `LAVALINK_PORT=2333`
- `LAVALINK_SECURED=false`
- `LAVALINK_PASSWORD`
- Optional `E2E_MUSIC_TEST_QUERY`; defaults to `Indila - Ainsi Bas La Vida (Marcoz Lima Remix)`.
- Optional `E2E_MUSIC_SECOND_TEST_QUERY`; defaults to `Daft Punk - Harder Better Faster Stronger`.
- PostgreSQL settings used by the E2E workflow

### Bot Runtime E2E - COMPLETE

- [x] Bot startup/shutdown connects with a real Discord token and resolves the configured guild.
- [x] Full bot process startup through `BotApplication` with real Discord, PostgreSQL, and Lavalink service graph.
- [x] Full guild-available initialization against live Discord and real persistence state.

### Command Flow E2E - COMPLETE FOR CURRENT TEXT COMMAND SCOPE

- [x] `!ping` in the test channel returns `Pong`.
- [x] `!unknowncommand` returns localized unknown-command error.
- [x] `!help` lists available commands.
- [x] Non-prefixed message is ignored.
- [x] Bot-authored command is ignored outside test mode.
- [x] Null prefix logs the expected error.
- [x] Register, unregister, duplicate register, and unregister-before-register logging paths are covered.

### Discord Wrapper E2E - COMPLETE

- [x] `DiscordChannelWrapper` maps real channel ID/name/guild and sends string/embed messages.
- [x] `DiscordGuildWrapper` maps real guild ID/name, members, and member lookup.
- [x] `DiscordMemberWrapper` maps bot flag, username, mention, and voice state.
- [x] `DiscordMessageWrapperFactory` maps real message properties and supports respond/modify.
- [x] `DiscordClientEventHandler` E2E-style startup dependency calls are covered.

### Reaction Flow E2E - COMPLETE FOR CURRENT AUTOMATED SCOPE

- [x] Sends reaction control message when track-started event is raised.
- [x] Reaction add in test mode calls expected Lavalink operation.
- [x] Reaction remove in test mode calls expected Lavalink operation.
- [x] Bot-authored reactions are ignored outside test mode.
- [x] Real Discord object context builds expected guild ID.
- [x] Full reaction flow with real Lavalink playback state.

### Music Flow E2E - CONFIG-GATED

- [x] Bot joins voice channel with real Lavalink when `DISCORD_TEST_VOICE_CHANNEL_ID` is configured.
- [x] `!play [query-or-url]` plays a configured track in a real voice channel when live music E2E settings are present.
- [x] Live music-flow test publishes visible Discord chat markers for each command step.
- [x] Live music-flow test registers the production `ReactionHandler`, sends the now-playing control message, and verifies progressive timer message updates.
- [x] `!pause` pauses current playback in the live music-flow test.
- [x] `!resume` resumes current playback in the live music-flow test.
- [x] `!skip` skips/stops current playback in the live music-flow test.
- [x] Track end auto-advances to the next track.
- [x] `!repeat` repeats current track.
- [x] `!repeatlist` cycles through queue snapshot.
- [x] `!leave` disconnects and clears player state in the live music-flow test.

### SlashCommand E2E - PIPELINE COMPLETE

- [x] `/play [query]` reaches the existing play command path via slash pipeline.
- [x] `/join` reaches the existing join command path via slash pipeline.
- [x] `/tag user:<member>` reaches the existing tag command path via slash pipeline.
- [x] `/ping` responds with `Pong!` via slash pipeline.
- [x] `/help` lists commands via slash pipeline.
- [x] `/skip`, `/pause`, `/resume`, and `/leave` reach the existing music control command paths.
- [x] `/queue` reaches the existing queue display command path.
- [x] `/shuffle` reaches the existing shuffle command path.
- [x] `/repeat track` and `/repeat list` reach the existing repeat command paths.
- [x] `/language hu` saves the selected language and returns a localized Hungarian response.
- [x] `/clear confirm:true` clears the queue through the existing clear command path.
- [x] `/clear confirm:false` does not clear the queue.

Live Discord slash invocation remains a manual/external-client validation area because bots cannot self-invoke application commands as a user.

Manual smoke checklist: `DC bot tests/EndToEndTests/Commands/SlashCommands/live_slash_invocation_smoke_checklist.md`.

---

## Priority Recommendations

### High Priority

1. Keep `DISCORD_TEST_VOICE_CHANNEL_ID` configured in CI/local E2E environments so the live Lavalink music-flow test exercises the real voice-channel path.
2. Execute the live slash invocation smoke checklist with a real Discord client before release.
3. Keep `E2E_MUSIC_TEST_QUERY` and `E2E_MUSIC_SECOND_TEST_QUERY` stable enough for queue, repeat, and repeat-list E2E coverage.

### Medium Priority

1. Keep PostgreSQL integration tests in sync with every new migration.
2. Add workflow smoke verification when Docker, Lavalink, or E2E config changes.

### Low Priority

1. Add exact coverage reports if required by branch policy.
2. Split very large unit suites if they become hard to maintain.
3. Replace manual status counts with generated reporting if the checklist becomes stale again.

---

## Current Verification Result

Last local verification run after the targeted integration expansion:

```text
dotnet build "DC bot.sln" --configuration Debug --no-restore

Warnings: 0
Errors: 0

dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "Category!=E2E"

Passed: 658
Failed: 0
Skipped: 0
```

Integration verification after adding targeted DI, command-handler, repository, music-service, track-ended, and localization coverage:

```text
dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "Category=Integration"

Passed: 41
Failed: 0
Skipped: 0
```

Focused live E2E verification for the newly automated gaps:

```text
dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "FullyQualifiedName~GuildAvailable_WithRealDiscordAndPostgreSql_InitializesGuildDataAndPlaybackState|FullyQualifiedName~TrackEnded_WithQueuedTrack_AutoAdvancesToNextTrack|FullyQualifiedName~RepeatCommand_WithRealPlayback_ReplaysCurrentTrackAfterTrackEnd|FullyQualifiedName~RepeatListCommand_WithRealPlayback_RequeuesSnapshotAfterQueueEnds|FullyQualifiedName~ReactionControls_WithRealPlayback_InvokeLavalinkOperations"

Passed: 5
Failed: 0
Skipped: 0
```

Focused live verification for the live music-flow fixture and play/pause/resume/skip/leave test:

```text
dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "FullyQualifiedName~MusicCommands_WithRealDiscordAndLavalink_PlayPauseResumeSkipLeave"

Passed: 1
Failed: 0
Skipped: 0
```

Previous full E2E suite run with real Discord configuration:

```text
dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "Category=E2E"

Passed: 68
Failed: 1
Skipped: 0

Failure:
DC_bot_tests.EndToEndTests.Service.ReactionHandlerEndToEndTests.OnReactionAdded_WhenBotAddsReaction_AndIsTestModeFalse_DoesNotCallLavaLinkOperations(emojiName: ":track_next:")
DSharpPlus.Exceptions.RateLimitException: Rate limited: TooManyRequests

Local host execution used `LAVALINK_HOSTNAME=127.0.0.1`; `lavalink` is only resolvable from inside the Docker network.
```

Slash command targeted verification:

```text
dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "FullyQualifiedName~SlashCommand"
Passed: 75
Failed: 0
Skipped: 0

dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "Category=Integration&FullyQualifiedName~SlashCommand"
Passed: 7
Failed: 0
Skipped: 0

dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --no-build --filter "Category=E2E&FullyQualifiedName~SlashCommand"
Passed: 16
Failed: 0
Skipped: 0
```

Full live playback execution is implemented and has targeted passing runs for `play/pause/resume/skip/leave`, track-end auto-advance, `!repeat`, `!repeatlist`, and reaction controls with real Discord voice, Lavalink, and PostgreSQL Testcontainers.

---

## Notes

- `readme_required_tests.md` should keep listing completed and missing test areas.
- `.gitignore` and `.dockerignore` policy belongs in the root README and repository hygiene docs, not as a replacement for this checklist.
- If a feature changes behavior, update this checklist in the same PR as the tests.
