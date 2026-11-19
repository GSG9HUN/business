# MusicControl

Maps guild_music_control_settings with a numeric(20,0) guild primary key, a default-false vote_skip_enabled column and nullable numeric DJ role.

## Overview

- Maps guild_music_control_settings with a numeric(20,0) guild primary key, a default-false vote_skip_enabled column and nullable numeric DJ role.

## Configurations

### GuildMusicControlSettingsConfiguration

**Purpose:** Maps guild_music_control_settings with a numeric(20,0) guild primary key, a default-false vote_skip_enabled column and nullable numeric DJ role.

**Files:** `GuildMusicControlSettingsConfiguration.cs`.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
