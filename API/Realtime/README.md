# Realtime

Hosts authenticated SignalR subscriptions and forwards persisted state changes to mobile clients.

## Overview

- Repositories publish PostgreSQL notifications; the listener loads snapshots and publishers send events to authorized per-user or per-user-guild groups.
- Subscription operations use Context.ConnectionAborted. Disconnect membership cleanup runs independently of that aborted token. Clients can refresh REST snapshots after reconnecting.

## Subfolders

### Events/

Defines immutable command-progress and guild snapshot messages sent to mobile clients.

**Files:**

- `BotControlCommandEvent.cs`
- `GuildBotStatusEvent.cs`
- `PlaybackSnapshotEvent.cs`
- `QueueSnapshotEvent.cs`

**Purpose:** Publishers attach repository-derived values or REST-compatible snapshots and send both feature-specific and generic event names where applicable.

---

### Listening/

Maintains a dedicated PostgreSQL LISTEN connection for command, playback, queue, and bot-status notifications.

**Files:**

- `MobileRealtimePostgresListener.cs`
- `PostgresNotification.cs`
- `PostgresRealtimeOptions.cs`

**Purpose:** MobileRealtimePostgresListener buffers channel/payload pairs, validates their identifiers, and creates a DI scope for publishing. It retries a failed listening connection until shutdown.

---

### Publishing/

Targets authorized mobile subscribers with command progress and fresh guild snapshots.

**Files:**

- `BotControlRealtimePublisher.cs`
- `GuildRealtimePublisher.cs`

**Purpose:** Command updates target the submitting user's guild group. Guild updates resolve currently authorized subscribed users before sending payloads.

---

### Snapshots/

Builds queue, playback, and bot-presence read models for publishers.

**Files:**

- `RealtimeSnapshotProvider.cs`

**Purpose:** RealtimeSnapshotProvider delegates playback to the shared playback snapshot service and maps queue and bot-status repository reads.

---

## Classes

### MobileUpdateGroups

**File:** `API/Realtime/MobileUpdateGroups.cs`

**Purpose:** Defines consistent per-user and per-user-guild SignalR group names.

**Methods:**

- `User` — Builds the SignalR group name for a single user's updates.

```csharp
internal static string User(ulong userId);
```

- `UserGuild` — Builds the SignalR group name scoped to both a guild and an individual user.

```csharp
internal static string UserGuild(ulong guildId, ulong userId);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = MobileUpdateGroups.User(userId);
```

---

### MobileUpdatesHub

**File:** `API/Realtime/MobileUpdatesHub.cs`

**Purpose:** Manages authenticated mobile subscriptions and their persisted guild memberships.

**Implements / inherits:** `Hub`.

**Methods:**

- `JoinGuildAsync` — Checks persisted guild access, joins the user-guild SignalR group, and stores the subscription.

```csharp
public async Task JoinGuildAsync(string guildId);
```

- `JoinUserUpdatesAsync` — Adds this authenticated connection to its per-user SignalR update group.

```csharp
public async Task JoinUserUpdatesAsync();
```

- `LeaveGuildAsync` — Removes the connection's user-guild SignalR group and persisted subscription.

```csharp
public async Task LeaveGuildAsync(string guildId);
```

- `LeaveUserUpdatesAsync` — Removes this authenticated connection from its per-user SignalR update group.

```csharp
public async Task LeaveUserUpdatesAsync();
```

- `OnConnectedAsync` — Logs the authenticated hub connection and delegates to the SignalR connection lifecycle.

```csharp
public override Task OnConnectedAsync();
```

- `OnDisconnectedAsync` — Removes persisted subscriptions independently of the aborted connection before completing disconnection.

```csharp
public override async Task OnDisconnectedAsync(Exception? exception);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await mobileUpdatesHub.JoinGuildAsync(guildId);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Realtime/Events/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Realtime/Listening/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Realtime/Publishing/README.md` — Detailed component contracts and examples for this subfolder.
- `API/Realtime/Snapshots/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot.Contracts/Interface/Service/Persistence/MobileApps/` — Defines repository operations for mobile identities, guild access, refresh sessions, and realtime subscription membership.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
