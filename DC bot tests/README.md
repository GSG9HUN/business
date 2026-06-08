# DC Bot Tests

This project contains the automated tests for the Discord bot.

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
- all 15 text commands resolve from the production startup graph

## Integration Coverage

The integration suite includes targeted coverage for:

- command-handler routing through the real text command list with fake Discord wrapper contexts
- direct PostgreSQL repository behavior for guild data, playback state, queue, and repeat-list storage
- `MusicQueueService`, `RepeatService`, `CurrentTrackService`, and `TrackEndedHandlerService` with real persistence and mocked external playback edges
- real English and Hungarian localization JSON loading for slash fallback texts

## E2E Notes

E2E tests depend on real Discord configuration and are not part of the default non-E2E verification command.

Required values depend on the specific test, but generally include:

- Discord bot token
- Discord test guild ID
- Discord test channel ID
- reachable Lavalink server

The slash command E2E pipeline tests do not invoke Discord as a user. They validate the local slash adapter/executor/text-command path because bots cannot self-invoke application commands.

Live music-flow E2E tests use `EndToEndTests/Service/LiveMusicFlowTestContext.cs` to keep real Discord, Lavalink, PostgreSQL, reaction handler, and command execution setup outside the scenario tests.

## Related Documentation

- `../DC bot/README.md` - application architecture
- `../DC bot/Startup/README.md` - startup composition
- `TestHelperFiles/README.md` - shared unit/integration/E2E test helpers
- `../readme_required_tests.md` - historical test coverage plan
