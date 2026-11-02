# Presentation

Formats playback metadata and sends now-playing notifications.

## Overview

- TrackFormatterService builds track text and embeds; TrackNotificationService handles response delivery through the selected message channel.
- Presentation errors should be diagnosed separately from the result of a remote audio command. The caller determines which text channel receives the notification.

## Classes

### TrackFormatterService

**File:** `DC bot/Service/Music/Presentation/TrackFormatterService.cs`

**Purpose:** Formats the current track followed by waiting tracks for repeat-mode responses.

**Implements / inherits:** `ITrackFormatterService`.

**Methods:**

- `FormatCurrentTrackListAsync` — Formats the current track followed by the waiting queue for repeat-mode responses.

```csharp
public async Task<string> FormatCurrentTrackListAsync(ulong guildId, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await trackFormatterService.FormatCurrentTrackListAsync(guildId, cancellationToken);
```

---

### TrackNotificationService

**File:** `DC bot/Service/Music/Presentation/TrackNotificationService.cs`

**Purpose:** Builds playback notifications and contains Discord delivery failures at the presentation boundary.

**Implements / inherits:** `ITrackNotificationService`.

**Methods:**

- `BuildNowPlayingEmbed` — Builds localized track details for the now-playing notification.

```csharp
public DiscordEmbed BuildNowPlayingEmbed(ILavaLinkTrack track, TimeSpan position, TimeSpan duration);
```

- `NotifyNowPlayingAsync` — Publishes started-track details to the selected response channel.

```csharp
public async Task NotifyNowPlayingAsync(
    IDiscordChannel textChannel,
    ILavaLinkTrack track,
    TimeSpan position,
    TimeSpan duration,
    CancellationToken cancellationToken = default);
```

- `NotifyQueueEmptyAsync` — Sends the localized queue-finished notification to the selected response channel.

```csharp
public async Task NotifyQueueEmptyAsync(IDiscordChannel textChannel, CancellationToken cancellationToken = default);
```

- `SendSafeAsync` — Attempts a Discord notification and logs delivery failures without failing playback.

```csharp
public async Task SendSafeAsync(
    IDiscordChannel channel,
    string message,
    string operation,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = trackNotificationService.BuildNowPlayingEmbed(track, position, duration);
```

---

## Related Components

- `DC bot/Service/Music/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Messaging/` — Represents Discord response delivery failures at the presentation boundary.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
