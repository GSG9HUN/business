# MusicControl

Carries immutable VoteSkipEnabled and DjRoleId values across the shared persistence boundary without Discord SDK dependencies.

## Overview

- Carries immutable VoteSkipEnabled and DjRoleId values across the shared persistence boundary without Discord SDK dependencies.

## Models

### GuildMusicControlSettingsRecord

**Purpose:** Carries immutable VoteSkipEnabled and DjRoleId values across the shared persistence boundary without Discord SDK dependencies.

**Files:** `GuildMusicControlSettingsRecord.cs`.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
