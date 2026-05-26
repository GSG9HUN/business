# DC Bot Tests

This project contains the automated tests for the Discord bot.

## Test Categories

- `Unit` - isolated tests with mocks and no external services
- `Integration` - service-graph and persistence-oriented tests; some use Testcontainers/PostgreSQL
- `E2E` - real Discord/Lavalink interaction tests that require external configuration

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

## Startup Coverage

`IntegrationTests/Service/ProgramIntegrationTests.cs` covers the startup refactor:

- missing `.env` continues with already-provided environment variables
- missing required environment values exits with a message
- `BotServiceProviderFactory` registers the core service graph
- the full startup service graph resolves against PostgreSQL
- `DatabaseMigrationRunner` applies pending EF Core migrations

## E2E Notes

E2E tests depend on real Discord configuration and are not part of the default non-E2E verification command.

Required values depend on the specific test, but generally include:

- Discord bot token
- Discord test guild ID
- Discord test channel ID
- reachable Lavalink server

## Related Documentation

- `../DC bot/README.md` - application architecture
- `../DC bot/Startup/README.md` - startup composition
- `../readme_required_tests.md` - historical test coverage plan
