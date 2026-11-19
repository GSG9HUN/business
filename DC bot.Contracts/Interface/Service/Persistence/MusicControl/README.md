# MusicControl

Stores default-disabled vote skip and an optional DJ role independently of 24/7, autoplay and presets. GetAsync returns disabled/null defaults; SetVoteSkipAsync and SetDjRoleAsync update only their own field.

## Overview

- Stores default-disabled vote skip and an optional DJ role independently of 24/7, autoplay and presets. GetAsync returns disabled/null defaults; SetVoteSkipAsync and SetDjRoleAsync update only their own field.

## Interfaces

### IGuildMusicControlSettingsRepository

**Purpose:** Stores default-disabled vote skip and an optional DJ role independently of 24/7, autoplay and presets. GetAsync returns disabled/null defaults; SetVoteSkipAsync and SetDjRoleAsync update only their own field.

**Files:** `IGuildMusicControlSettingsRepository.cs`.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
