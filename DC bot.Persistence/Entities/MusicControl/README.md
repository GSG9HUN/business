# MusicControl

Represents one guild-owned settings row with VoteSkipEnabled defaulting to false and an optional DjRoleId.

## Overview

- Represents one guild-owned settings row with VoteSkipEnabled defaulting to false and an optional DjRoleId.

## Entities

### GuildMusicControlSettingsEntity

**Purpose:** Represents one guild-owned settings row with VoteSkipEnabled defaulting to false and an optional DjRoleId.

**Files:** `GuildMusicControlSettingsEntity.cs`.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
