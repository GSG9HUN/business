# DC bot.Contracts

This class library defines the shared language used by the Discord bot, mobile API, and persistence implementation. It contains repository interfaces, immutable storage models, bot-command payloads and identifiers, music-provider definitions, and shared playlist-name validation.

## Overview

- `TwentyFourSevenSettingsRecord` and `IGuildDataRepository` expose persistent voice preferences independently of observed connection status.
- Public member documentation defines persisted queue and command states, supported settings values, bot command type identifiers, and realtime protocol event names.
- Define repository operations and immutable storage models.
- Share bot-command payloads, identifiers, and event names across processes.
- Reuse provider URL rules and playlist-name validation without SDK or database dependencies.

## Runtime Flow

```text
DC bot / API
  -> repository interfaces and shared values
  <- DC bot.Persistence implements storage

API -> command type + serialized payload -> PostgreSQL -> bot worker
Bot/persistence state -> shared event name -> API realtime publisher
```

The project targets .NET 9 and has no project or package references. It does not depend on EF Core, Npgsql, ASP.NET Core, DSharpPlus, or Lavalink implementations. Its explicit namespace root is `DC_bot`, so shared types keep that prefix even though the folder and project file are named `DC bot.Contracts`.

## Subfolders

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

## Contract Semantics

Repository absence, ordering, conditional mutation, and cancellation behavior should be explicit. Queue start claims carry operation identity; a late completion must not commit against a newer claim. Canonical batch items preserve the playback list independently of waiting queue positions. Previous lookup is limited to the active batch, while history reads can list recent batches without changing playback.

Records at this boundary are not tracked database entities and are not automatically HTTP response shapes. An API mapper chooses its own client contract, and a repository chooses how an entity is stored. The shared value must remain meaningful without either implementation detail.

Command and event names form a cross-process protocol. Adding or changing a payload requires coordinated handling in the API producer and bot consumer, including input validation and compatibility during deployment.

## Maintenance Boundaries

- Keep SQL, table mapping, migrations, and provider exceptions in Persistence; translate expected uniqueness conflicts into the shared exception.
- Keep HTTP request and response DTOs in API.
- Keep live player sessions, Discord wrappers, and command orchestration in the bot.
- Pass cancellation through repository I/O; an implementation may retain an independent lifetime for required cleanup.
- Provider definitions describe supported naming and URLs. Deployment switches and node capabilities decide actual search and enqueue availability.

When a shared interface or record changes, update its implementation and consumers together, build the solution, and run the relevant behavior tests. Folder READMEs explain the feature boundary and link the current types rather than tracking a change log.

## Related Components

- `DC bot/` — Discord commands, live playback lifecycle, and the mobile command worker.
- `API/` — HTTP request handling, authentication, response mapping, and SignalR publication.
- `DC bot.Contracts/` — Shared storage interfaces, immutable models, command payloads, and protocol names.
- `DC bot.Persistence/` — PostgreSQL repository implementations, migrations, and database notifications.
- `DC bot tests/` — Behavior tests and explicitly skipped planned cases.
- `AGENTS.md` — Implementation and detailed README conventions.
