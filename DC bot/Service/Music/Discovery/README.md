# Discovery

Implements Discord lyrics, personal favorites, provider recommendations, bounded autoplay and opt-in search selection.

## Overview

- Lyrics pages include localized title, track/artist and page number; favorites display bold actionable IDs and track titles. Searchpicker sends a single result list/select, without a separate waiting response, timeout instruction or cancel button. The internal 45-second deadline, owner/voice checks and final control removal remain.

- Favorite add/remove/play, autoplay changes and search-picker changes carry explicit localized outcome-title keys. Read-only state keeps its informational title; presentation does not infer suboperations from translated body text.

- Resolves lyrics, personal favorites and source-based recommendations.
- Coordinates optional autoplay and expiring Discord search selection.
- Persists user and guild preferences independently of runtime player state.

## Services

### LyricsService

**Purpose:** Reads the configured LavaLyrics plugin through the authenticated MusicDiscovery HTTP client. This uses the existing node/plugin and configured HTTP pipeline.

**Methods:** `ExecuteAsync` accepts an optional query and `--page N`; it returns line-aware pages of at most 1,600 characters with Discord navigation metadata. Buttons reuse the loaded text, and `--page N` selects the initial page. A query resolves one track; playlists are rejected. Null text can fall back to provider line objects. Missing/provider failures return a localized unavailable result.

**Usage:**

```text
<prefix>lyrics
<prefix>lyrics song title --page 2
/lyrics query:song title page:2
```

**Validation Checks:** Pages start at one; no playback mutation occurs. Provider availability depends on the configured node plugins and source support. [LavaLyrics API](https://github.com/topi314/LavaLyrics#api).

---

### FavoriteService

**Purpose:** Saves personal source tracks independently of guild playlists or mobile account registration.

Favorite playback records the authenticated caller as a canonical Discord mention, allowing requester skip checks to use stable identity rather than a display name. Favorites and opt-in preferences are durable; autoplay history and chain budgets are process-local.

**Methods:** `ExecuteAsync` handles add, list, remove and play. Add without a query captures the current track; a query uses its first single-track result. IDs are stable, duplicate source identities reuse an ID, and list pages contain at most ten entries. `CountAsync` supplies owner-scoped totals; page callbacks clamp the requested page and load only its ten records. Pagination metadata accompanies the ordinary localized message and does not change saved-track capacity.

**Usage:**

```text
<prefix>favorite add
<prefix>favorite add artist song
<prefix>favorite list 2
<prefix>favorite play 41
<prefix>favorite remove 41
```

**Validation Checks:** All reads/deletes/play lookups include the requesting Discord user ID. Explicit playback requires the current voice context and uses the normal queue/start results, including capacity and uncertain outcomes.

---

### RecommendationService

**Purpose:** Provides up to five real source recommendations and optional queue continuation. Spotify seeds use the configured LavaSrc track mix; YouTube seeds use the installed YouTube plugin's radio mix. Other sources first resolve a YouTube seed. Provider failures produce unavailable results, not arbitrary search-result substitutions.

Recommendations and autoplay status carry informational result metadata; autoplay changes retain success presentation.

**Methods:** `ExecuteAsync` lists recommendations; `AutoplayAsync` reads/sets the durable guild preference; `ManualRequest` resets the automatic chain and invalidates an in-flight decision; `ObserveEnded` retains the last twenty ended identities; `TryEnqueueAsync` conditionally enqueues one automatic item.

**Validation Checks:** Autoplay defaults off. Changes require the bot voice channel. Natural track completion attempts continuation only after the explicit waiting queue and repeat behavior are exhausted. Atomic empty-queue insertion prevents overwriting a concurrent manual append. Generated items have requester `autoplay`. Recent source/display identities and waiting items are excluded. At most ten automatic or uncertain insertions are budgeted until a manual play/playnext/favorite-play request or explicit setting change resets the chain. A known no-insert result releases its slot; an uncertain commit retains it. Ended non-automatic items also reset the chain, covering existing playlist/mobile enqueue paths that bypass the Discord request adapter. Recent history and counters reset on process restart; the opt-in preference survives.

**Usage:** `autoplay on`, `autoplay off`, `autoplay status`, `recommendations [query]`.

**Related provider contracts:** [LavaSrc 4.8.3](https://github.com/topi314/LavaSrc/tree/4.8.3#supported-urls-and-queries), [configured YouTube source](https://github.com/lavalink-devs/youtube-source/blob/f45bbb7aebfcbc1c553769e04af6cd43afa8b7c3/common/src/main/java/dev/lavalink/youtube/YoutubeAudioSourceManager.java).

---

### SearchPickerHandler

**Purpose:** Owns single-use, 45-second Discord select menus.

One prompt and selected/closed messages use informational embeds; denials retain their existing contextual response. No separate waiting message or cancellation button is sent. The prompt retains its five-result result menu; denials stay ephemeral. If the prompt embed is rejected, bounded text is retried with the same result menu. Closing the picker removes active controls and also supports text fallback. Cancellation after sending still retains the message for cleanup.

**Methods:** `ConfigureAsync` handles personal on/off/status; `SelectAsync` returns the first result when disabled and offers up to five real results when enabled; `CancelPending` supersedes a user's old request; `OnComponentInteractionAsync` validates component submissions.

**Validation Checks:** Disabled by default. PlaybackRequestService bypasses playlists and direct HTTP/HTTPS links for both play and playnext, while explicit search prefixes still enter selection. Menus bind user, guild, message and player generation; voice membership is checked on interaction and again before returning the choice. Timeout, caller cancellation and supersession never queue a fallback. The chosen URI becomes reload metadata so selecting a later result cannot reload the original query's first result.

**Usage:** `searchpicker on`, `searchpicker off`, `searchpicker status`. Slash requests use the same behavior; the selection menu is posted to the command channel after a localized acknowledgement.

## Related Components

- `DC bot/Service/Music/Playback/` - Existing request/start ownership and queue results.
- `DC bot/Service/Music/Events/TrackEndedHandlerService.cs` - Repeat/manual queue precedence and natural-end autoplay hook.
- `DC bot.Persistence/Repositories/Discovery/` - Personal favorites and durable preference storage.
- `DC bot/Commands/TextCommands/Music/` and `DC bot/Commands/SlashCommands/Music/` - Registered adapters.
- `docs/plans/mobile-music-parity-roadmap.md` - Deferred mobile work.
