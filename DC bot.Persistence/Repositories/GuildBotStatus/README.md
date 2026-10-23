# GuildBotStatus

Implements shared storage operations for persisted bot voice presence, channel display metadata, and voice-user counts.

## Overview

- Callers resolve the contracts through AddPersistenceServices; implementations create contexts through IDbContextFactory and map database state into shared records.
- Forward caller cancellation through database I/O. Preserve transactional ownership and use the shared retry/rollback helpers where a multi-step mutation requires them.

## Classes

### GuildBotStatusRealtimeNotifier

**File:** `DC bot.Persistence/Repositories/GuildBotStatus/GuildBotStatusRealtimeNotifier.cs`

**Purpose:** Publishes guild bot status realtime notifications within the caller's database transaction.

**Methods:**

- `NotifyGuildBotStatusUpdatedAsync` — Publishes a guild voice-status event through the current database transaction when the provider is relational.

```csharp
public static Task NotifyGuildBotStatusUpdatedAsync(
    BotDbContext dbContext,
    ulong guildId,
    string eventName,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await GuildBotStatusRealtimeNotifier.NotifyGuildBotStatusUpdatedAsync(
    dbContext,
    guildId,
    eventName,
    cancellationToken);
```

---

### GuildBotStatusRepository

**File:** `DC bot.Persistence/Repositories/GuildBotStatus/GuildBotStatusRepository.cs`

**Purpose:** Reads and mutates guild bot status state using short-lived contexts from the injected EF factory.

**Implements / inherits:** `IGuildBotStatusRepository`.

**Methods:**

- `GetByGuildIdsAsync` — Reads persisted voice status for the requested guild identifiers without tracking the returned entities.

```csharp
public async Task<IReadOnlyDictionary<ulong, GuildBotStatusRecord>> GetByGuildIdsAsync(
    IReadOnlyCollection<ulong> guildIds,
    CancellationToken cancellationToken = default);
```

- `MarkDisconnectedVoiceAsync` — Clears the guild's connected voice-channel state and publishes the disconnect event.

```csharp
public async Task MarkDisconnectedVoiceAsync(ulong guildId, CancellationToken cancellationToken = default);
```

- `UpsertConnectedVoiceAsync` — Persists the guild's connected voice channel and user count, then publishes the matching voice event.

```csharp
public async Task UpsertConnectedVoiceAsync(
    ulong guildId,
    ulong voiceChannelId,
    string voiceChannelName,
    int voiceUserCount,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await guildBotStatusRepository.GetByGuildIdsAsync(guildIds, cancellationToken);
```

---

## Related Components

- `DC bot.Persistence/Repositories/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Db/` — Owns the EF Core context, design-time creation, and startup migration runner.
- `DC bot.Persistence/Entities/GuildBotStatus/` — Represents the database rows used for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Persistence/Repositories/Shared/` — Provides PostgreSQL transaction retries and owned rollback handling shared by repositories.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
