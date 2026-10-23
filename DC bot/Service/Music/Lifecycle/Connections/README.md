# Connections

Joins or resolves Lavalink players and waits for validated voice readiness.

## Overview

- Keep the existing join deadline, late-completion observation, and failed-join cleanup policy.
- Validate registered player identity and voice readiness before exposing the joined session.

## Connection Services

### PlayerConnectionService

**File:** `DC bot/Service/Music/Lifecycle/Connections/PlayerConnectionService.cs`

**Purpose:** Resolves and validates players while observing late joins and coordinating failed-join cleanup.

**Implements / inherits:** `IPlayerConnectionService`.

**Methods:**

- `TryGetAndValidateExistingPlayerAsync` — Resolves and validates the existing player without creating a voice connection.

```csharp
public async Task<(
    IDiscordMessage message,
    IDiscordChannel? channel,
    CancellationToken cancellationToken = default);
```

- `TryJoinAndValidateAsync` — Joins or resolves the voice player and waits for readiness within the caller's join deadline.

```csharp
public async Task<(
    IDiscordMessage message,
    IDiscordChannel? channel,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playerConnectionService.TryGetAndValidateExistingPlayerAsync(message, channel, cancellationToken);
```

**Behavior:** A caller deadline stops waiting but does not prove that the SDK join had no remote effect. Late completion is observed and failed or superseded players are cleaned up through the lifecycle coordinator.


---

### PlayerConnectionRetryPolicy

**File:** `DC bot/Service/Music/Lifecycle/Connections/PlayerConnectionRetryPolicy.cs`

**Purpose:** Waits for joined-player readiness while rejecting replaced players and honoring the join deadline.

**Methods:**

- `ValidateJoinedPlayerAsync` — Polls readiness until validation succeeds, the player is replaced, or cancellation is requested.

```csharp
internal async Task<ConnectionValidationResult> ValidateJoinedPlayerAsync(
    IAudioService audioService,
    ulong guildId,
    ILavalinkPlayer? joinedConnection,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await playerConnectionRetryPolicy.ValidateJoinedPlayerAsync(
    audioService,
    guildId,
    joinedConnection,
    cancellationToken);
```


---

## Related Components

- `DC bot/Service/Music/Lifecycle/README.md` - Parent lifecycle overview and folder navigation.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Shared join and cleanup deadlines.
- `DC bot/Service/Music/Lifecycle/Models/` - Session, reservation, and recovery data contracts.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Runtime registration.
