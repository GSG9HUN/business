# Recovery

Reconciles gateway voice state, durable playback starts, and authenticated Lavalink evidence.

## Overview

- Use bounded gateway recovery for non-persistent guilds and pending-start reconciliation before another join.
- Query remote evidence rather than treating SDK registry absence as proof of disconnection.

## Recovery Services

### DiscordPlaybackRecoveryService

**File:** `DC bot/Service/Music/Lifecycle/Recovery/DiscordPlaybackRecoveryService.cs`

**Purpose:** Restores remembered non-persistent voice connections during a bounded gateway-session recovery window. Enabled persistent voice guilds are delegated to the periodic worker.

**Methods:**

- `OnBotVoiceStateUpdatedAsync` — Updates a remembered channel or requests disconnect recovery inside the active window.

```csharp
public Task OnBotVoiceStateUpdatedAsync(ulong guildId, ulong? channelId);
```

- `OnSessionCreatedAsync` — Opens a bounded recovery window for remembered sessions after a new gateway session.

```csharp
public Task OnSessionCreatedAsync(IEnumerable<ulong> guildIds);
```

- `Remember` — Remembers the current player and channel as the permitted gateway recovery target.

```csharp
public void Remember(PlaybackSession session, ulong channelId);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordPlaybackRecoveryService.OnBotVoiceStateUpdatedAsync(guildId, channelId);
```

**Behavior:** For non-persistent guilds, this service uses a remembered current player and a bounded gateway window; it does not recreate a missing player. Enabled persistent voice guilds are handled by `PersistentVoiceRecoveryService`, including disconnects outside that gateway window.


---

### PlaybackRecoveryService

**File:** `DC bot/Service/Music/Lifecycle/Recovery/PlaybackRecoveryService.cs`

**Purpose:** Reconciles durable pending starts with remote player evidence before another attempt may begin.

**Methods:**

- `TryConfirmStartAsync` — Checks remote player evidence to determine whether a durable pending start succeeded.

```csharp
public async Task<bool> TryConfirmStartAsync(
    PlaybackSession session,
    ControlReservation reservation,
    Guid operationId,
    long queueItemId,
    string nodeSessionId,
    string encodedTrack,
    string? previousEncodedTrack);
```

- `CommitKnownStartAsync` — Commits an observed remote start to queue and current-track persistence under the session fence.

```csharp
public async Task<bool> CommitKnownStartAsync(
    PlaybackSession session,
    long queueItemId,
    Guid operationId,
    string serializedTrack,
    PlaybackStartReason reason,
    TimeSpan? initialPosition = null,
    bool initiallyPaused = false);
```

- `RecoverBeforeJoinAsync` — Reconciles stale local and remote playback state before a new join is permitted.

```csharp
public async Task<bool> RecoverBeforeJoinAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `RecoverPendingAsync` — Reconciles unresolved durable starts against the current player and remote node state.

```csharp
public async Task RecoverPendingAsync(CancellationToken cancellationToken = default);
```

- `RunAsync` — Periodically reconciles durable pending playback starts until application shutdown, logging failed scans without ending the loop.

```csharp
public async Task RunAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackRecoveryService.CommitKnownStartAsync(session, queueItemId, operationId, serializedTrack, reason);
```


---

### PlaybackRemoteProbe

**File:** `DC bot/Service/Music/Lifecycle/Recovery/PlaybackRemoteProbe.cs`

**Purpose:** Queries Lavalink REST state to confirm voice identity, pending starts, and expired node sessions.

**Properties:**

```csharp
// The Lavalink node base address derived from the configured host, port, and TLS setting.
public string NodeAddress { get; }
```

**Methods:**

- `IsPlayingAsync` — Checks whether the current guild player has active track playback.

```csharp
public async Task<bool> IsPlayingAsync(
    string sessionId,
    ulong guildId,
    string encodedTrack,
    CancellationToken cancellationToken = default);
```

- `IsSessionGoneAsync` — Checks remote session evidence before treating an old node session as absent.

```csharp
public async Task<bool> IsSessionGoneAsync(string nodeAddress, string sessionId, CancellationToken cancellationToken = default);
```

- `IsVoiceConnectedAsync` — Checks remote connection state against guild, Discord voice session, and available channel identity.

```csharp
public async Task<bool> IsVoiceConnectedAsync(
    string nodeSessionId,
    ulong guildId,
    string discordVoiceSessionId,
    ulong voiceChannelId,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playbackRemoteProbe.IsPlayingAsync(sessionId, guildId, encodedTrack, cancellationToken);
```


**Additional methods:**

```csharp
public Task<bool> IsPlayerGoneAsync(
    string nodeSessionId,
    ulong guildId,
    CancellationToken cancellationToken = default);

public Task StartPausedAsync(
    string nodeSessionId,
    ulong guildId,
    string encodedTrack,
    TimeSpan position,
    CancellationToken cancellationToken = default);
```

**Purpose:** Confirm an old player is absent using authenticated session/player evidence, or apply a paused start inside an already bound transport fence. A missing SDK registration alone is not remote evidence. Unknown in-flight starts retain their stronger reconciliation requirements.

---

## Related Components

- `DC bot/Service/Music/Lifecycle/README.md` - Parent lifecycle overview and folder navigation.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Shared join and cleanup deadlines.
- `DC bot/Service/Music/Lifecycle/Models/` - Session, reservation, and recovery data contracts.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Runtime registration.
