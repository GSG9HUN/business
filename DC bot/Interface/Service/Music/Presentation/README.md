# Presentation

Defines the presentation boundary used by the bot music pipeline.

## Overview

- Implementations live under Service/Music/Presentation; commands and orchestrators exchange these contracts instead of constructing transport or persistence dependencies themselves.
- Result states distinguish completed, rejected, pending, and superseded work where applicable. Preserve ownership and cancellation semantics when adding an operation.

## Interfaces

### ITrackFormatterService

**File:** `DC bot/Interface/Service/Music/Presentation/ITrackFormatterService.cs`

**Purpose:** Defines the track formatter service boundary so callers are independent of its concrete implementation.

**Methods:**

- `FormatCurrentTrackListAsync` — Formats the current track followed by the waiting queue for repeat-mode responses.

```csharp
Task<string> FormatCurrentTrackListAsync(ulong guildId, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await trackFormatterService.FormatCurrentTrackListAsync(guildId, cancellationToken);
```

---

### ITrackNotificationService

**File:** `DC bot/Interface/Service/Music/Presentation/ITrackNotificationService.cs`

**Purpose:** Defines the track notification service boundary so callers are independent of its concrete implementation.

**Methods:**

- `BuildNowPlayingEmbed` — Builds localized track details for the now-playing notification.

```csharp
DiscordEmbed BuildNowPlayingEmbed(ILavaLinkTrack track, TimeSpan pos, TimeSpan trackDuration);
```

- `NotifyNowPlayingAsync` — Publishes started-track details to the selected response channel.

```csharp
public Task NotifyNowPlayingAsync(
    IDiscordChannel textChannel,
    ILavaLinkTrack track,
    TimeSpan position,
    TimeSpan duration,
    CancellationToken cancellationToken = default);
```

- `NotifyQueueEmptyAsync` — Sends the localized queue-finished notification to the selected response channel.

```csharp
public Task NotifyQueueEmptyAsync(IDiscordChannel textChannel, CancellationToken cancellationToken = default);
```

- `SendSafeAsync` — Attempts a Discord notification and logs delivery failures without failing playback.

```csharp
public Task SendSafeAsync(
    IDiscordChannel channel,
    string message,
    string operation,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = trackNotificationService.BuildNowPlayingEmbed(track, pos, trackDuration);
```

---

## Related Components

- `DC bot/Interface/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Music/Tracks/` — Defines the tracks boundary used by the bot music pipeline.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
