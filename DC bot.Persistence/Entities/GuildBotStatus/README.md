# GuildBotStatus

Represents the database rows used for persisted bot voice presence, channel display metadata, and voice-user counts.

## Overview

- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### GuildBotStatusEntity

**File:** `DC bot.Persistence/Entities/GuildBotStatus/GuildBotStatusEntity.cs`

**Purpose:** Represents the persisted guild bot status row mapped by the EF Core model.

**Properties:**

```csharp
// The Discord guild identifier.
public ulong GuildId { get; set; }
public string? ConnectedVoiceChannelName { get; set; }
public int ConnectedVoiceUserCount { get; set; }
public DateTimeOffset UpdatedAtUtc { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/Guilds/` — Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
