# GuildBotStatus

Defines shared records and state values for persisted bot voice presence, channel display metadata, and voice-user counts.

## Overview

- Persistence maps entities into these values, and application services or API mappers consume them without database-provider dependencies.
- Keep wire response DTOs in API and tracked entities in DC bot.Persistence. Changing a record affects both repository implementations and their consumers.

## Models and Results

### GuildBotStatusRecord

**File:** `DC bot.Contracts/Interface/Service/Persistence/Models/GuildBotStatus/GuildBotStatusRecord.cs`

**Purpose:** Carries immutable guild bot status values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public record GuildBotStatusRecord(
    ulong GuildId,
    bool IsConnectedToVoice,
    string? ConnectedVoiceChannelName,
    int ConnectedVoiceUserCount,
    DateTimeOffset UpdatedAtUtc);
```

---

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/Models/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/README.md` — Project setup and architecture.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
