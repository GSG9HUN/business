# Listening

Maintains a dedicated PostgreSQL LISTEN connection for command, playback, queue, and bot-status notifications.

## Overview

- MobileRealtimePostgresListener buffers channel/payload pairs, validates their identifiers, and creates a DI scope for publishing. It retries a failed listening connection until shutdown.
- PostgresRealtimeOptions is injected through IOptions. A notification is a signal to reload state, not the full state itself. Cancellation must stop waiting and publishing during shutdown.

## Classes

### MobileRealtimePostgresListener

**File:** `API/Realtime/Listening/MobileRealtimePostgresListener.cs`

**Purpose:** Listens for PostgreSQL change notifications and publishes scoped mobile updates with reconnect handling.

**Implements / inherits:** `BackgroundService`.

---

## Models and Results

### PostgresNotification

**File:** `API/Realtime/Listening/PostgresNotification.cs`

**Purpose:** Carries a PostgreSQL notification channel and payload to the listener's processing queue.

**Record declaration:**

```csharp
internal sealed record PostgresNotification(string Channel, string Payload);
```

---

### PostgresRealtimeOptions

**File:** `API/Realtime/Listening/PostgresRealtimeOptions.cs`

**Purpose:** Provides the PostgreSQL connection string used by the dedicated realtime listener.

**Record declaration:**

```csharp
public sealed record PostgresRealtimeOptions(string ConnectionString);
```

---

## Related Components

- `API/Realtime/README.md` — Parent feature overview and neighboring components.
- `API/README.md` — Project setup and architecture.
- `API/Realtime/Publishing/Interface/` — Defines command-progress and guild-snapshot publication operations.
- `DC bot.Contracts/Interface/Service/Persistence/BotControl/` — Defines repository operations for durable bot commands, their claim/completion state, and cross-process change signals.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
