# Controls

Coordinates durable guild settings, current-attempt voting, five-minute empty-channel cleanup and read-only music status through session, repository, localization and leave boundaries.

## Overview

- Coordinates guild voting settings, DJ-role selection and track-scoped skip votes.
- Maintains empty-channel deadlines through the playback lifecycle.
- Provides read-only music and connection status.

## Services

### MusicControlSettingsService

**Purpose:** Coordinates durable guild settings, current-attempt voting, five-minute empty-channel cleanup and read-only music status through existing session, repository, localization and leave boundaries.

Automatic empty-channel leave and background vote completion publish localized status embeds. Failed embed delivery falls back to plain text bounded to 2000 characters.

**Files:**

- `MusicControlSettingsService.cs`

**Methods:** `ConfigureAsync`, `EvaluateSkipAsync`, `MaintainAsync` and `StatusAsync`.

**Usage:**

```text
music-config show
music-config vote-skip on
music-config vote-skip off
music-config dj-role @DJ
music-config dj-role clear
music-status
skip
```

Slash equivalents are `/music-config show`, `/music-config vote-skip enabled:true`, `/music-config dj-role role:DJ` (native role picker; omit to clear), `/music-status` and `/skip`. Text examples omit the configured prefix.

**Validation Checks:**

- Settings mutations require Manage Guild/Administrator, can run before joining voice, and persist across restart. The everyone role and roles outside the current guild are rejected.
- Voting defaults off. With voting enabled, the requester and configured DJ may skip immediately from the bot voice channel. Other humans have one vote per ready track attempt. The threshold is half the humans rounded upward; every bot is excluded.
- Departing listeners lose their votes; the five-second recovery loop rechecks changed membership and can complete an already sufficient vote. Background skip is bound to its original attempt and cannot skip a replacement track. Direct skipto requires requester/DJ authorization while voting is enabled.
- New Discord/mobile play, playnext, saved-playlist and favorite loads save a canonical Discord mention in the existing requester field. Legacy username-only rows cannot safely establish ownership; their listeners can still vote and DJs can skip.
- Empty channels leave after five continuous minutes, sampled by the five-second recovery worker. Human return events cancel the timer. Enabling 24/7 cancels it; the shared leave gate rechecks 24/7, current generation and listener presence before admitting full cleanup. Maintenance runs within `PersistentVoiceRecoveryService`.
- Successful empty-channel leave posts the English/Hungarian reason to the remembered music text channel. Failed notification delivery is logged; it does not undo completed cleanup. Cleanup failure does not produce a false success message.
- Music status displays actual connection phase, channels, track, volume, filter names, queue size, repeat, voting progress, DJ role, 24/7 and the empty-channel countdown. It does not promise a recovery retry time unavailable through the current state boundary.
- Fresh joins use neutral effects and 100% output; the volume range is 0-200%.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
