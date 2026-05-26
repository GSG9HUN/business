# Required Tests - DC Bot

This file tracks which test areas are already covered and which test areas are still required. It is a living checklist, not only a command reference.

## Executive Summary

**Current automated test inventory:**

- Unit test methods in source: **531**
- Integration test methods in source: **22**
- E2E test methods in source: **40**
- Total tracked test methods: **593**

The source-method count and the `dotnet test` result count differ because xUnit `[Theory]` tests can produce multiple executed test cases from one method.

**Current status:**

- COMPLETE: Core text commands, music services, persistence, startup composition, wrappers, validation, localization, logging/error paths.
- COMPLETE: PostgreSQL persistence integration coverage through Testcontainers.
- PARTIAL: E2E coverage exists for Discord lifecycle, command messages, reaction handling, and wrappers.
- PENDING: Slash command unit tests and slash command E2E tests.
- PENDING: Full real Lavalink music playback E2E flow.

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

### Exceptions - COMPLETE

- [x] `BotException`
- [x] `LocalizationException`
- [x] `MessageSendException`
- [x] `LavalinkOperationException`
- [x] `QueueOperationException`
- [x] `TrackLoadException`
- [x] `ValidationException`

### SlashCommands - PENDING

These classes still need a framework-aware testing approach because DSharpPlus slash command classes are harder to instantiate and exercise cleanly in isolated unit tests.

- [ ] `PlaySlashCommand` - defers response when invoked.
- [ ] `PlaySlashCommand` - validates user voice channel.
- [ ] `PlaySlashCommand` - handles URL vs query input.
- [ ] `PlaySlashCommand` - responds with success or validation error.
- [ ] `TagSlashCommand` - finds member by username.
- [ ] `TagSlashCommand` - returns error when member is not found.
- [ ] `TagSlashCommand` - tags member successfully.
- [ ] `PingSlashCommand` - responds with `Pong!`.
- [ ] `HelpSlashCommand` - lists available commands.

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
- [x] Full startup graph resolves against PostgreSQL.
- [x] `DatabaseMigrationRunner` rejects in-memory migration provider.
- [x] `DatabaseMigrationRunner` applies pending PostgreSQL migrations.

### PostgreSQL Persistence - COMPLETE

- [x] Playback queue and repeat state survive repository recreation.
- [x] `QueueRepository.ClaimNextQueuedItemAsync` claims the lowest queued item.
- [x] Concurrent claim does not claim the same queued item twice.
- [x] Reorder works against PostgreSQL unique position index without constraint collision.

### CommandHandlerService Integration - COMPLETE

- [x] Register/unregister behavior.
- [x] Unknown command response.
- [x] DI command resolution.
- [x] Prefix validation and logging.

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

---

## Required E2E Tests

E2E tests require real Discord configuration and are excluded from normal PR verification. The E2E workflow starts PostgreSQL and Lavalink containers, mounts `lavalink-server/application.yaml`, and runs `Category=E2E` by default.

Required E2E settings:

- `DISCORD_TOKEN`
- `DISCORD_TEST_GUILD_ID`
- `DISCORD_TEST_CHANNEL_ID`
- `LAVALINK_HOSTNAME=lavalink`
- `LAVALINK_PORT=2333`
- `LAVALINK_SECURED=false`
- `LAVALINK_PASSWORD`
- PostgreSQL settings used by the E2E workflow

### Bot Runtime E2E - PARTIAL

- [x] Bot startup/shutdown connects with a real Discord token and resolves the configured guild.
- [ ] Full bot process startup through `BotApplication` with real Discord, PostgreSQL, and Lavalink service graph.
- [ ] Full guild-available initialization against live Discord and real persistence state.

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

### Reaction Flow E2E - PARTIAL

- [x] Sends reaction control message when track-started event is raised.
- [x] Reaction add in test mode calls expected Lavalink operation.
- [x] Reaction remove in test mode calls expected Lavalink operation.
- [x] Bot-authored reactions are ignored outside test mode.
- [x] Real Discord object context builds expected guild ID.
- [ ] Full reaction flow with real Lavalink playback state.

### Music Flow E2E - PENDING

- [ ] Bot joins voice channel with real Lavalink.
- [ ] `!play [URL]` plays a track in a real voice channel.
- [ ] `!play [query]` searches and plays a track in a real voice channel.
- [ ] `!pause` pauses current playback.
- [ ] `!resume` resumes current playback.
- [ ] `!skip` skips to the next queued track.
- [ ] Track end auto-advances to the next track.
- [ ] `!repeat` repeats current track.
- [ ] `!repeatlist` cycles through queue snapshot.
- [ ] `!leave` disconnects and clears player state.

### SlashCommand E2E - PENDING

- [ ] `/play [query]` plays track via slash command.
- [ ] `/tag [username]` tags member via slash command.
- [ ] `/ping` responds with `Pong!`.
- [ ] `/help` lists commands.

---

## Priority Recommendations

### High Priority

1. Add slash command tests or extract slash command logic into testable services.
2. Add real Lavalink music-flow E2E tests for play/pause/resume/skip/leave.
3. Add full `BotApplication` E2E startup coverage with real external services.

### Medium Priority

1. Keep PostgreSQL integration tests in sync with every new migration.
2. Add E2E coverage for repeat and repeat-list behavior after basic playback E2E is stable.
3. Add workflow smoke verification when Docker, Lavalink, or E2E config changes.

### Low Priority

1. Add exact coverage reports if required by branch policy.
2. Split very large unit suites if they become hard to maintain.
3. Replace manual status counts with generated reporting if the checklist becomes stale again.

---

## Current Verification Result

Last local verification run:

```text
dotnet test "DC bot tests\DC bot tests.csproj" --configuration Debug --filter "Category!=E2E"

Passed: 576
Failed: 0
Skipped: 0
```

E2E tests were not run locally in this pass because they require real Discord/Lavalink configuration.

---

## Notes

- `readme_required_tests.md` should keep listing completed and missing test areas.
- `.gitignore` and `.dockerignore` policy belongs in the root README and repository hygiene docs, not as a replacement for this checklist.
- If a feature changes behavior, update this checklist in the same PR as the tests.
