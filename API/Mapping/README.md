# Mapping

Converts repository records and service outcomes into HTTP response contracts.

## Overview

- TrackResponseMapper decodes stored Lavalink identities; queue and playlist mapping omits undecodable tracks. DomainToHttpMapper handles expected API failures.
- These are explicit, manual mapping strategies. Mapping should not open database contexts, send Discord messages, or execute playback commands.

## Classes

### BotControlCommandHttpMapper

**File:** `API/Mapping/BotControlCommandHttpMapper.cs`

**Purpose:** Maps durable bot command records into accepted responses and polling status payloads.

**Methods:**

- `ToAccepted` — Returns HTTP Accepted with the durable command identifier and its polling location.

```csharp
internal static IResult ToAccepted(BotControlCommandRecord command);
```

- `ToStatusResponse` — Maps persisted command state, result, error, and lifecycle timestamps into a polling response.

```csharp
internal static BotControlCommandStatusResponse ToStatusResponse(BotControlCommandRecord command);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = BotControlCommandHttpMapper.ToAccepted(command);
```

---

### DomainToHttpMapper

**File:** `API/Mapping/DomainToHttpMapper.cs`

**Purpose:** Translates API success and error results into HTTP status codes and response bodies.

**Methods:**

- `ToHttpResult` — Maps a success value or API error code to its corresponding HTTP result.

```csharp
public static IResult ToHttpResult<T>(ApiResult<T> result);
```

---

### GuildResponseMapper

**File:** `API/Mapping/GuildResponseMapper.cs`

**Purpose:** Maps stored Discord memberships and bot presence to guild responses and CDN icon URLs.

**Methods:**

- `MapBotStatus` — Maps stored bot presence with disconnected defaults when no status exists.

```csharp
public static GuildBotStatusResponse MapBotStatus(GuildBotStatusRecord? botStatus);
```

- `MapGuild` — Combines guild membership details, access level, icon URL, and bot presence into a response.

```csharp
public static GuildSummaryResponse MapGuild(MobileAppUserGuildRecord guild, GuildBotStatusRecord? botStatus = null);
```

- `MapGuildIconUrl` — Builds the guild icon URL from stored Discord icon metadata.

```csharp
public static string? MapGuildIconUrl(MobileAppUserGuildRecord guild);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = GuildResponseMapper.MapBotStatus(botStatus);
```

---

### QueueResponseMapper

**File:** `API/Mapping/QueueResponseMapper.cs`

**Purpose:** Builds queue responses from persisted items, excluding undecodable track payloads.

**Methods:**

- `Map` — Maps decodable queued tracks into a response with contiguous one-based positions.

```csharp
public static QueueResponse Map(ulong guildId, IReadOnlyList<QueueItemRecord> queueItems);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = QueueResponseMapper.Map(guildId, queueItems);
```

---

### TrackResponseMapper

**File:** `API/Mapping/TrackResponseMapper.cs`

**Purpose:** Decodes Lavalink tracks into API payloads and clamps duration values to supported integer seconds.

**Methods:**

- `TryMapPlaybackTrack` — Decodes the current track into playback metadata and requester information, or returns null.

```csharp
internal static PlaybackTrackResponse? TryMapPlaybackTrack(string trackIdentifier, string? requestedBy = null);
```

- `TryMapPlaylistTrack` — Decodes a playlist track into its stored order and metadata, or returns null.

```csharp
internal static PlaylistTrackResponse? TryMapPlaylistTrack(PlaylistTrackRecord trackRecord);
```

- `TryMapQueueTrack` — Decodes a queued track into its position, metadata, duration, and requester, or returns null.

```csharp
internal static QueueTrackResponse? TryMapQueueTrack(string trackIdentifier, int position, string? requestedBy = null);
```

- `SumDurations` — Sums decodable playlist durations in seconds, saturating at the maximum integer value.

```csharp
internal static int SumDurations(IReadOnlyCollection<PlaylistTrackRecord> tracks);
```

- `ToDurationSeconds` — Converts a duration to nonnegative whole seconds, saturating at the maximum integer value.

```csharp
internal static int ToDurationSeconds(TimeSpan duration);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = TrackResponseMapper.SumDurations(tracks);
```

---

## Related Components

- `API/README.md` — Parent feature overview and neighboring components.
- `API/Errors/` — Defines error categories understood by HTTP result mapping.
- `API/Results/` — Represents either a successful value or an expected error before conversion to HTTP.
- `API/Responses/BotControl/` — Defines client-facing output for durable bot commands, their claim/completion state, and cross-process change signals.
- `API/Responses/Guilds/` — Defines client-facing output for guild defaults, premium state, and the mapped premium-audit schema.
- `DC bot.Contracts/` — Shared storage and command protocol used by handlers and services.
