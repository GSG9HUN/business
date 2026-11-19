# MusicControl

Reads guild-scoped immutable settings and uses tracked field assignments plus SaveChangesAsync with serializable conflict retries. Changing voting preserves the DJ role, and changing the role preserves voting.

## Overview

- Reads guild-scoped immutable settings and uses tracked field assignments plus SaveChangesAsync with serializable conflict retries. Changing voting preserves the DJ role, and changing the role preserves voting.

## Repositories

### GuildMusicControlSettingsRepository

**Purpose:** Reads guild-scoped immutable settings and uses tracked field assignments plus SaveChangesAsync with serializable conflict retries. Changing voting preserves the DJ role, and changing the role preserves voting.

**Files:** `GuildMusicControlSettingsRepository.cs`.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
