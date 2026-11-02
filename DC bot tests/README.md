# DC Bot Tests

This project contains the automated tests for the Discord bot.

`UnitTests/Api/PlaylistArtworkTests.cs` verifies saved playlist cover selection and per-track artwork in API responses, including missing artwork and invalid persisted metadata.

## Test Categories

- `Unit` - isolated tests with mocks and no external services
- `Integration` - service-graph and persistence-oriented tests; some use Testcontainers/PostgreSQL
- `E2E` - real Discord/Lavalink interaction tests plus local pipeline-level E2E tests; real external tests require configuration

## Common Commands

Run unit tests:

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Unit"
```

Run integration tests:

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Integration"
```

Run the normal verification set:

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category!=E2E"
```

Run slash command coverage:

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "FullyQualifiedName~SlashCommand"
```

## Startup Coverage

`IntegrationTests/Service/ProgramIntegrationTests.cs` covers the startup refactor:

- missing `.env` continues with already-provided environment variables
- missing required environment values exits with a message
- `BotServiceProviderFactory` registers the core service graph
- the full startup service graph resolves against PostgreSQL
- `DatabaseMigrationRunner` applies pending EF Core migrations
- slash command services, modules, and `SlashCommandProcessor` resolve from DI
- all 24 registered text commands resolve from the production startup graph
- command routing uses the injected `ICommandRegistry` from the startup graph

## Integration Coverage

The integration suite includes targeted coverage for:

- command-handler routing through the real text command list with fake Discord wrapper contexts, split into message, utility, music, and queue routing integration files
- reaction handler DI and dispatch wiring across the split reaction services
- direct PostgreSQL repository behavior for guild data, playback state, queue, repeat-list storage, and saved playlists
- `MusicQueueService`, `RepeatService`, `CurrentTrackService`, and `TrackEndedHandlerService` with real persistence and mocked external playback edges
- `PlaylistService` and playlist repositories with mocked Lavalink edges or PostgreSQL-backed storage
- queue state assertions use the explicit `QueueItemState` contract
- real English and Hungarian localization JSON loading for command and slash fallback texts

## E2E Notes

E2E tests depend on real Discord configuration and are not part of the default non-E2E verification command.

Required values depend on the specific test, but generally include:

- Discord bot token
- Discord test guild ID
- Discord test channel ID
- reachable Lavalink server

The slash command E2E pipeline tests do not invoke Discord as a user. They validate the local slash adapter/executor/text-command path for music, queue, playlist, and utility commands because bots cannot self-invoke application commands.

Playlist text-command E2E tests validate the local message command handler pipeline for create, list, view, load, remove-song, rename, and delete playlist flows without relying on live Discord.
Playlist slash-command E2E tests validate `/playlist create/save/list/view/load/add-song/remove-song/rename/delete` through the same text command pipeline, including delete confirmation.

Live music-flow E2E tests use `EndToEndTests/Service/LiveMusicFlowTestContext.cs` as a scenario-facing facade. `MusicFlowEndToEndTests.cs` and `ReactionHandlerEndToEndTests.cs` contain the live scenarios; `DiscordE2EClientFixture.cs`, `MusicFlowDriver.cs`, `LavalinkE2EFixture.cs`, and `LiveDiscordMessageProbe.cs` hold the focused setup, command driver, Lavalink wait, and Discord message probing responsibilities.

## Related Documentation

- `../DC bot/README.md` - application architecture
- `../DC bot/Startup/README.md` - startup composition
- `TestHelperFiles/README.md` - shared unit/integration/E2E test helpers
- `../readme_required_tests.md` - historical test coverage plan

## Custom Playlist Cover Verification

`UnitTests/Api/PlaylistArtworkTests.cs` covers custom-cover precedence, URL validation, authorization, and clearing. `IntegrationTests/Persistence/PlaylistRepositoryPostgreSqlIntegrationTests.SetArtworkAsync_PersistsAndClearsCover` verifies PostgreSQL migration and repository read/write behavior when Docker is available.

## Saved Track URL Regression Coverage

`UnitTests/Service/Music/Playlist/PlaylistServiceTrackUriTests.cs` verifies playlist imports persist individual track URLs and song additions persist resolved URLs rather than input references. Missing URLs remain empty without losing serialized track identity.

## Playlist Command Execution

The new `PlaylistCreationTests` and `PlaylistBotControlHandlerTests` contain test scaffolds only. All scenarios are explicitly skipped with implementation pending; they do not provide behavior coverage.

## Queue Repeat Synchronization

QueueRepeatListSynchronizationTests contains skipped integration-test scaffolds for remove/reorder/shuffle, duplicate occurrences, clear, disabled repeat, and transaction rollback. These scaffolds do not provide behavior coverage.

## Queue Batches and History

QueueBatchPersistenceTests contains explicitly skipped scaffolds for batch identity, duplicate occurrences, reorder membership, current/history display, and waiting-only command indices. The existing in-memory repository adapter implements the new display-read contract for compilation.

## Queue Snapshot Playlist Saving

QueuePlaylistSnapshotTests and PlaylistSnapshotTransactionTests contain skipped scaffolds for ordering, duplicates, advancement, guild access, limits, and transaction rollback. No behavior coverage is claimed for these scaffolds.
