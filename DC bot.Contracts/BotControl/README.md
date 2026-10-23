# BotControl

Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.

## Overview

- The API writes commands using these names and Payloads; the worker reads them and the API publishes state updates using the shared event identifiers.
- Changing a command or event identifier affects both processes and mobile clients. Keep this protocol independent of ASP.NET, Discord SDK objects, and EF entities.

## Subfolders

### Payloads/

Defines the serialized arguments stored with durable commands submitted by the API and consumed by the bot worker.

**Files:**

- `PlaylistCommandPayload.cs`
- `PlaylistImportCommandPayload.cs`
- `PlaylistTrackCommandPayload.cs`
- `QueueEnqueueCommandPayload.cs`
- `QueueMoveToIndexCommandPayload.cs`
- `QueueRemoveCommandPayload.cs`

**Purpose:** Queue payloads describe play, move, and removal input. Playlist payloads describe a saved playlist target, an imported URL, or a track to resolve.

---

## Classes

### BotControlCommandTypes

**File:** `DC bot.Contracts/BotControl/BotControlCommandTypes.cs`

**Purpose:** Defines stable command type names for the durable API-to-bot command bridge.

---

### MobileRealtimeEventNames

**File:** `DC bot.Contracts/BotControl/MobileRealtimeEventNames.cs`

**Purpose:** Defines stable realtime event names shared by persistence, API, bot, and mobile consumers.

---

## Models and Results

### BotControlCommandChannelContext

**File:** `DC bot.Contracts/BotControl/BotControlCommandChannelContext.cs`

**Purpose:** Carries immutable bot control command channel context values across the shared storage or mobile-command boundary.

**Record declaration:**

```csharp
public sealed record BotControlCommandChannelContext(ulong? VoiceChannelId, ulong? TextChannelId);
```

---

## Related Components

- `DC bot.Contracts/README.md` — Parent feature overview and neighboring components.
- `DC bot.Contracts/BotControl/Payloads/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Persistence/` — Repository implementations and database mappings that consume these shared contracts.
