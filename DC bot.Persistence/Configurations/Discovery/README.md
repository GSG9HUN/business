# Discovery

Maps favorites and discovery preferences to PostgreSQL tables.

## Overview

- Discord owner identifiers use the existing numeric(20,0) conversion.
- Settings default false; favorite source identity is unique per owner.

## Configurations

### FavoriteTrackConfiguration

**Purpose:** Maps FavoriteTrackEntity to favorite_track with generated bigint ID and a unique index on user_id/track_key. Metadata remains source text rather than live SDK objects.

---

### UserMusicSettingsConfiguration

**Purpose:** Maps user_music_settings with a non-generated user_id key and search_picker_enabled default false.

---

### GuildMusicSettingsConfiguration

**Purpose:** Maps guild_music_settings with a non-generated guild_id key and autoplay_enabled default false.

## Related Components

- `DC bot.Persistence/Db/BotDbContext.cs` - Applies all three configurations.
- `DC bot.Persistence/Entities/Discovery/` - Tracked properties.
- `DC bot.Persistence/Migrations/` - Generated schema and snapshot.
