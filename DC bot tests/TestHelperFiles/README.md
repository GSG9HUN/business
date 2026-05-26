# Test Helper Files

This folder contains shared helpers used by the test project.

## Files

- `TrackTestHelper.cs` - creates reusable test track data for music service and queue-related tests

## Guidance

Keep helpers deterministic and free of external service dependencies. Helpers used by E2E tests should live near the E2E tests if they require real Discord or Lavalink configuration.
