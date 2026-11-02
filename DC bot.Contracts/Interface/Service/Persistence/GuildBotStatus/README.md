# GuildBotStatus

Defines repository operations for persisted bot voice presence, channel display metadata, and voice-user counts.

## Overview

- The bot and API consume these interfaces; concrete implementations and DI registration live in DC bot.Persistence unless noted below.
- Describe lookup absence, ordering, conditional mutation, and cancellation in the contract. Keep provider-specific SQL and EF types out of this boundary.

## Interfaces

### IGuildBotStatusRepository

**File:** `DC bot.Contracts/Interface/Service/Persistence/GuildBotStatus/IGuildBotStatusRepository.cs`

**Purpose:** Defines shared storage operations for guild bot status repository, including caller cancellation and immutable result contracts.

**Methods:**

- `GetByGuildIdsAsync` — Reads persisted voice status for the requested guild identifiers without tracking the returned entities.

```csharp
Task<IReadOnlyDictionary<ulong, GuildBotStatusRecord>> GetByGuildIdsAsync(
    IReadOnlyCollection<ulong> guildIds,
    CancellationToken ct = default);
```

- `MarkDisconnectedVoiceAsync` — Clears the guild's connected voice-channel state and publishes the disconnect event.

```csharp
Task MarkDisconnectedVoiceAsync(ulong guildId, CancellationToken ct = default);
```

- `UpsertConnectedVoiceAsync` — Persists the guild's connected voice channel and user count, then publishes the matching voice event.

```csharp
Task UpsertConnectedVoiceAsync(
    ulong guildId,
    ulong voiceChannelId,
    string voiceChannelName,
    int voiceUserCount,
    CancellationToken ct = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await guildBotStatusRepository.GetByGuildIdsAsync(guildIds, ct);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Contracts/Interface/Service/Persistence/Models/GuildBotStatus/` — Defines shared records and state values for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
