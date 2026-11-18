# DC bot.Contracts

This class library defines the shared language used by the Discord bot, mobile API, and persistence implementation. It contains repository interfaces, immutable storage models, bot-command payloads and identifiers, music-provider definitions, and shared playlist-name validation.

## Overview

- Defines SDK-independent data and operation contracts shared by the bot and API.
- Provides persistence interfaces, immutable records, validation and durable command payloads.

## Subfolders

### Configuration/

Defines shared configuration for persisted playlist capacity.

**Files:**

- `PlaylistOptions.cs`

**Purpose:** Defaults non-premium guilds to 5 saved playlists, 100 tracks per playlist and 100 tracks per import. Active premium bypasses count caps; naming, ownership, uniqueness and storage validation still apply.

---

### BotControl/

Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.

**Files:**

- `BotControlCommandChannelContext.cs`
- `BotControlCommandTypes.cs`
- `MobileRealtimeEventNames.cs`

**Purpose:** The API writes commands using these names and Payloads; the worker reads them and the API publishes state updates using the shared event identifiers.

---

### Interface/

Groups contracts consumed across project boundaries.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** The Service/Persistence tree defines operations implemented by DC bot.Persistence and used by the bot and API.

---

### Music/

Groups provider definitions used by both bot input resolution and API search.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** The Search subfolder defines provider IDs, prefixes, source-manager names, supported URLs, and capability metadata.

---

### Validation/

Normalizes and validates playlist names consistently across bot and API entry points.

**Files:**

- `PlaylistNameValidator.cs`

**Purpose:** PlaylistNameValidator checks the name before a caller reaches a repository or submits a playlist command.

---

## Related Components

- `DC bot/` — Discord commands, live playback lifecycle, and the mobile command worker.
- `API/` — HTTP request handling, authentication, response mapping, and SignalR publication.
- `DC bot.Contracts/` — Shared storage interfaces, immutable models, command payloads, and protocol names.
- `DC bot.Persistence/` — PostgreSQL repository implementations, migrations, and database notifications.
- `DC bot tests/` — Behavior tests and explicitly skipped planned cases.
- `AGENTS.md` — Implementation and detailed README conventions.
