# Handlers

Coordinates HTTP input, authenticated user context, persistence contracts, and API response mapping.

## Overview

- ApiUserContext resolves the JWT subject and checks stored guild access before feature handlers read or change guild data.
- Handlers use shared repository contracts rather than opening EF contexts. Mutating playback commands are persisted for the bot worker instead of executed by the API.

## Subfolders

### Auth/

Coordinates Discord OAuth, mobile login-ticket exchange, refresh-token rotation, and logout.

**Files:**

- `AuthHandler.cs`

**Purpose:** The browser callback consumes OAuth state, fetches Discord identity and guild memberships, persists the user, and redirects with a short-lived ticket. Exchange creates a mobile session; refresh conditionally rotates its stored token hash.

---

### BotControl/

Reads progress and results of durable bot-control commands.

**Files:**

- `BotControlCommandHandler.cs`

**Purpose:** The handler loads the command, verifies access to its guild, and only returns progress to the user who submitted it.

---

### Guilds/

Lists guilds visible to the authenticated mobile user and assembles their bot-presence metadata.

**Files:**

- `GuildHandler.cs`

**Purpose:** Memberships come from the mobile user repository; bot voice status is loaded separately and mapped into the guild response.

---

### MusicSearch/

Authorizes provider capability discovery and track or playlist searches.

**Files:**

- `MusicSearchHandlers.cs`

**Purpose:** After guild-access validation, the handler calls IMusicSearchService and maps provider failures and retry hints into safe HTTP responses. Repeated search query parameters are rejected.

---

### Playback/

Accepts playback control requests and explicit repeat-mode changes for an authorized guild.

**Files:**

- `PlaybackHandlers.cs`

**Purpose:** Command execution requests become durable bot-control rows. SetRepeatModeAsync directly updates repeat flags and repeat-list storage through repositories.

---

### Player/

Returns the playback read model for a guild visible to the authenticated user.

**Files:**

- `PlayerHandler.cs`

**Purpose:** Guild access is checked before IPlaybackSnapshotService assembles track, queue, repeat, and bot-presence data.

---

### Playlists/

Provides authorized playlist reads, creation, rename, deletion, and track operations.

**Files:**

- `PlaylistHandlers.cs`

**Purpose:** Metadata operations use playlist repositories. Track resolution, URL imports, and loading into playback are queued as durable bot commands.

---

### Profile/

Reads the authenticated user's profile and applies partial mobile preference updates.

**Files:**

- `ProfileHandler.cs`

**Purpose:** GetMyProfile loads identity only; GetProfile also loads settings. UpdateSettings normalizes supplied language and theme values before patching preferences.

---

### Queue/

Reads a guild queue and submits authorized queue mutation commands.

**Files:**

- `QueueHandlers.cs`

**Purpose:** Enqueue validates the query and optional channel identifiers, resolves requester display metadata, and stores a bot play command. Clear, remove, shuffle, and move also use the command bridge.

---

### Status/

Maps database connectivity checks to an operational HTTP response.

**Files:**

- `StatusHandler.cs`

**Purpose:** The handler calls IDbStatusCheck with the request token. Connectivity failures use the existing database-unavailable response; requested cancellation propagates.

---

## Classes

### ApiUserContext

**File:** `API/Handlers/ApiUserContext.cs`

**Purpose:** Resolves the authenticated Discord user and checks persisted guild access before handler operations.

**Methods:**

- `TryGetDiscordUserId` — Tries to read the authenticated Discord user identifier from the subject claim.

```csharp
internal static bool TryGetDiscordUserId(HttpContext httpContext, out ulong discordUserId);
```

- `RequireGuildAccessAsync` — Checks the subject claim and persisted membership, returning an HTTP access error when required.

```csharp
internal static async Task<(
    HttpContext httpContext,
    IMobileAppUserRepository userRepository,
    ulong guildId,
    CancellationToken cancellationToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = ApiUserContext.TryGetDiscordUserId(httpContext, out var discordUserId);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Handlers/Auth/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Guilds/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/MusicSearch/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Playback/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Player/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Playlists/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Profile/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Queue/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Handlers/Status/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/` — Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
