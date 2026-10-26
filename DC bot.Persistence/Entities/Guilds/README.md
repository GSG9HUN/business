# Guilds

Represents the database rows used for guild defaults, premium state, and the mapped premium-audit schema.

## Overview

- `IsTwentyFourSevenEnabled`, `PreferredVoiceChannelId`, and `PreferredTextChannelId` are updated by the guild repository; disabling preserves the channel preferences.
- Feature repositories read and mutate these values in short-lived EF contexts, then map results into DC bot.Contracts records.
- Entity properties describe persisted state. Keep HTTP response formatting, Discord objects, and playback transport behavior outside these types.

## Classes

### GuildDataEntity

**File:** `DC bot.Persistence/Entities/Guilds/GuildDataEntity.cs`

**Purpose:** Represents the persisted guild data row mapped by the EF Core model.

**Properties:**

```csharp
// The Discord guild identifier.
public ulong GuildId { get; set; }
public bool IsPremium { get; set; }
public DateTimeOffset? PremiumUntilUtc { get; set; }
public DateTimeOffset UpdatedAtUtc { get; set; }
public bool IsTwentyFourSevenEnabled { get; set; }
public ulong? PreferredVoiceChannelId { get; set; }
public ulong? PreferredTextChannelId { get; set; }
public GuildPlaybackStateEntity? PlaybackState { get; set; }
public GuildBotStatusEntity? BotStatus { get; set; }
public ICollection<GuildQueueItemEntity> QueueItems { get; set; }
public ICollection<GuildRepeatListItemEntity> RepeatListItems { get; set; }
public ICollection<GuildPremiumAuditEntity> PremiumAuditEntries { get; set; }
public ICollection<PlaylistEntity> Playlists { get; set; }
public ICollection<UserGuildEntity> UserGuilds { get; set; }
```

The twenty-four-seven fields store the desired voice connection mode and notification channels. They do not themselves create a player, reconnect voice, or resume playback; those behaviors require runtime services.

---

### GuildPremiumAuditEntity

**File:** `DC bot.Persistence/Entities/Guilds/GuildPremiumAuditEntity.cs`

**Purpose:** Represents the persisted guild premium audit row mapped by the EF Core model.

**Properties:**

```csharp
// The identifier of this persisted record.
public long Id { get; set; }
public ulong GuildId { get; set; }
public long? ChangedByUserId { get; set; }
public bool OldIsPremium { get; set; }
public bool NewIsPremium { get; set; }
public DateTimeOffset ChangedAtUtc { get; set; }
public string? Note { get; set; }
public GuildDataEntity Guild { get; set; }
```

---

## Related Components

- `DC bot.Persistence/Entities/README.md` — Parent feature overview and neighboring components.
- `DC bot.Persistence/README.md` — Project setup and architecture.
- `DC bot.Persistence/Entities/GuildBotStatus/` — Represents the database rows used for persisted bot voice presence, channel display metadata, and voice-user counts.
- `DC bot.Persistence/Entities/MobileApps/` — Represents the database rows used for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Persistence/Entities/Playback/` — Represents the database rows used for current playback state, repeat flags, start ownership, and canonical batch metadata.
- `DC bot.Persistence/Entities/Playlists/` — Represents the database rows used for saved playlist metadata and ordered, serialized playlist tracks.
- `DC bot.Contracts/Interface/Service/Persistence/` — Shared interfaces and immutable values implemented by persistence.
