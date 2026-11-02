# Playback

Accepts playback control requests and explicit repeat-mode changes for an authorized guild.

## Overview

- Command execution requests become durable bot-control rows. SetRepeatModeAsync directly updates repeat flags and repeat-list storage through repositories.
- The repeat-mode endpoint has its own persistence path; it does not itself perform a Discord or Lavalink operation. Keep this distinction when changing bot and mobile behavior together.

## Classes

### PlaybackHandlers

**File:** `API/Handlers/Playback/PlaybackHandlers.cs`

**Purpose:** Accepts authorized playback commands and persists explicit repeat-mode changes.

**Methods:**

- `ExecuteAsync` — Persists an authorized playback command for asynchronous execution by the bot.

```csharp
public static async Task<IResult> ExecuteAsync(
    HttpContext httpContext,
    IBotControlCommandsRepository repository,
    IMobileAppUserRepository userRepository,
    CancellationToken ct);
```

- `SetRepeatModeAsync` — Validates the requested repeat mode and persists repeat flags and the repeat-list snapshot.

```csharp
public static async Task<IResult> SetRepeatModeAsync(
    HttpContext httpContext,
    SetRepeatModeRequest request,
    IMobileAppUserRepository userRepository,
    IPlaybackStateRepository playbackStateRepository,
    IQueueRepository queueRepository,
    IRepeatListRepository repeatListRepository,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await PlaybackHandlers.ExecuteAsync(httpContext, repository, userRepository, ct);
```

---

## Related Components

- `API/Handlers/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Mapping/` — Converts repository records and service outcomes into HTTP response contracts.
- `API/Requests/Playback/` — Defines client-supplied input for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/Interface/Service/Persistence/BotControl/` — Defines repository operations for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
