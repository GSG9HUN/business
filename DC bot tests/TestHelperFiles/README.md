# Test Helper Files

This folder contains shared helpers used by the test project.

## Files

- `AsyncEnumerableAwaiterExtensions.cs` - awaits async event streams in tests
- `DiscordClientDisposeHelper.cs` - disposes Discord clients while tolerating disconnected gateway state
- `DiscordEventArgsFactory.cs` - builds DSharpPlus event args for tests
- `ServiceProviderDisposeHelper.cs` - disposes service providers while ignoring known Discord disconnect cleanup noise
- `SlashCommandTestGraph.cs` - builds the shared slash-command-to-text-command test graph for slash unit and E2E pipeline tests
- `TestDiscordClientFactory.cs` - creates Discord clients for test scenarios
- `TestSlashInteractionContext.cs` - in-memory slash interaction context for slash command tests
- `TrackTestHelper.cs` - creates reusable test track data for music service and queue-related tests

## Guidance

Keep helpers deterministic and free of external service dependencies unless they are explicitly E2E helpers. Shared test graph builders should centralize common wiring without hiding assertions in the helper. Helpers that require real Discord or Lavalink configuration should keep that dependency obvious in the test name or folder.
