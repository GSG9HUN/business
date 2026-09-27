# Player Connection Unit Tests

This folder covers `PlayerConnectionService` voice-channel connection behavior.

## Files

### PlayerConnectionServiceTestBase.cs

Shared fixture for player connection tests. It creates mocks for `IAudioService`, `IPlayerManager`, `IValidationService`, `ILavalinkNodeConnectionService`, `IResponseBuilder`, guild/channel wrappers, and guild bot status persistence.

### PlayerConnectionServiceJoinTests.cs

Covers basic join validation:

- missing voice channel
- invalid guild or channel IDs
- player validation failure
- connection validation failure
- successful join and guild voice status update
- Lavalink join exceptions

### PlayerConnectionServiceJoinRetryTests.cs

Covers join readiness retry behavior:

- connection validation retries are attempted before returning an error
- cancellation during retry is propagated
- stale disconnected players are disconnected before a new join
- a player that appears in the manager on a later retry is accepted
- all-attempt failure sends a single validation error

### PlayerConnectionServiceExistingPlayerTests.cs

Covers playback-control lookup behavior for an already joined player:

- missing voice channel
- failed player lookup
- disconnected player rejection
- successful existing-player validation
- validation exceptions mapped to Lavalink errors

## Expected Behavior

After `JoinAsync`, the service should not immediately fail just because the first player lookup is not yet connected. The retry path validates both the returned join connection and the guild player from the manager. This protects first-call `join` and `play` flows where the bot has joined voice but Lavalink state settles slightly later.

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "FullyQualifiedName~PlayerConnectionService"
```
