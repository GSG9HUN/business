# Controls

Coordinates durable guild settings, current-attempt voting, five-minute empty-channel cleanup and read-only music status through session, repository, localization and leave boundaries.

## Overview

- StatusAsync displays compact Voice/Lavalink/Queue information. Lavalink readiness requires a successful live server-information HTTP request with a three-second deadline; transport failures/timeouts report unavailable and caller cancellation propagates. Without an active API client it reports not checked.

- Coordinates guild voting settings, DJ-role selection and track-scoped skip votes.
- Maintains empty-channel deadlines through the playback lifecycle.
- Provides read-only music and connection status.

## Services

### MusicControlSettingsService

**Purpose:** Coordinates durable guild settings, current-attempt voting, five-minute empty-channel cleanup and read-only music status through existing session, repository, localization and leave boundaries.

Automatic empty-channel leave and background vote completion delegate presentation and delivery to `ITrackNotificationService.NotifyMusicControlAsync`. The control service determines the outcome and localized reason; the notification service applies playback-cleared/skip titles and bounded text fallback.

**Files:**

- `MusicControlSettingsService.cs`

**Methods:** `ConfigureAsync`, `EvaluateSkipAsync`, `MaintainAsync` and `StatusAsync`. Read-only settings and status carry informational presentation; saved setting changes retain success presentation.

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
- Music status displays compact voice/API health/queue information. API health is checked remotely; it does not imply that a track is ready to play.
- Fresh joins use neutral effects and 100% output; the volume range is 0-200%.

## Related Components

- `DC bot.Contracts/Interface/Service/Persistence/MusicControl/` - Shared settings operations.
- `DC bot.Persistence/Db/BotDbContext.cs` - Shared settings storage model.
- `DC bot/Service/Music/Controls/` - Runtime voting, configuration and status.
