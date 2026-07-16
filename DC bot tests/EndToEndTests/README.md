# End-to-End Tests

This folder contains tests that can interact with real Discord and Lavalink resources.

## Scope

E2E tests are allowed to:

- connect a real `DiscordClient`
- send or inspect real Discord messages
- use configured Discord channels
- depend on a reachable Lavalink server
- validate local end-to-end command pipelines when external Discord self-invocation is not possible

Because they depend on external services and timing, they are excluded from the normal non-E2E verification command.

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=E2E"
```

## Configuration

`EndToEndTestConfiguration.cs` reads the environment values needed by E2E tests. Missing configuration is treated as a
config-gated no-op: tests write the missing requirement to output and return without failing when the external test
environment is not available.

Commonly required values include:

- Discord bot token
- Discord test channel ID
- reachable Lavalink server

## Related Tests

- `Service/` - bot lifecycle and reaction flow
- `Service/MusicFlowEndToEndTests.cs` and `Service/ReactionHandlerEndToEndTests.cs` - live music-flow and reaction-control scenarios
- `Service/LiveMusicFlowTestContext.cs` - live music-flow facade that composes real Discord, Lavalink, command driver, message probe, and cleanup helpers
- `Service/DiscordE2EClientFixture.cs`, `Service/MusicFlowDriver.cs`, `Service/LavalinkE2EFixture.cs`, `Service/LiveDiscordMessageProbe.cs` - split helpers behind the live music-flow facade
- `Service/Core/` - real command handling split into registration, live-message, and guard E2E tests with `CommandHandlerEndToEndTestBase`
- `Commands/TextCommands/Playlist/` - local text command handler pipeline for playlist command flows, including remove-song routing
- `Commands/SlashCommands/` - local slash adapter -> executor -> text command pipeline, split by Music/Queue/Utility command domains
- `Wrapper/` - DSharpPlus wrapper behavior against real Discord objects
