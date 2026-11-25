# BotControl

Executes API-originated commands from the durable PostgreSQL command bridge.

## Overview

Saved-playlist loading captures the requester username alongside the existing Discord mention so mobile clients can display a name without Discord-specific markup.
`set_volume` validates a typed `SetVolumeCommandPayload` and forwards the percentage through `HandlePlaybackControlAsync` to the existing volume service. HTTP acceptance, audio failure, uncertain outcomes, and terminal success remain separate.

The dispatcher accepts the typed `seek` payload, validates its position and attempt, and forwards it through `HandlePlaybackControlAsync` to the fenced `ILavaLinkService.SeekAsync` overload. Existing Discord context and same-voice authorization remain in force.


- Playlist import calls `IPlaylistService.ImportPlaylistAsync` and consumes `ImportPlaylistResult`. The durable command identifier retains its existing wire value.

- Executes durable API-originated commands through bot service boundaries.
- Resolves authenticated Discord context and response destinations.
- Maps execution outcomes and notifications to stored command results.

## Classes

### PlaylistBotControlHandler

**Purpose:** Validates saved tracks, records the authenticated requester as a canonical Discord mention, and loads them through the joined session/event pipeline.

**File:** `PlaylistBotControlHandler.cs`.

Idle-player loads succeed only after `Started`; other outcomes retain `playbackStatus`, `operationId`, `queueItemId` and `requiresReconciliation` metadata. Uncertain starts are not blindly retried or enqueued again. Loads into a playing player succeed after enqueueing. `QueueFull` reports no inserted incoming tracks.

---

### BotControlCommandDispatcher

**File:** `DC bot/Service/BotControl/BotControlCommandDispatcher.cs`

**Purpose:** Dispatches durable mobile commands to playback services and produces structured outcomes.

**Implements / inherits:** `IBotControlCommandDispatcher`.

**Methods:**

- `DispatchAsync` — Dispatches the operation to its resolved service handler.

```csharp
public Task<BotControlCommandResult> DispatchAsync(
    BotControlCommandRecord command,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlCommandDispatcher.DispatchAsync(command, cancellationToken);
```

---

### BotControlContextResolver

**File:** `DC bot/Service/BotControl/BotControlContextResolver.cs`

**Purpose:** Resolves guild, member, and channel permissions for durable mobile commands.

Response candidates are the remembered music channel, explicit request text channel and requesting user voice chat, in that order. Each must belong to the guild and support messages. Remembered destinations are process-local and cleared by explicit leave.

**Implements / inherits:** `IBotControlContextResolver`.

**Methods:**

- `ResolveAsync` — Resolves the command's guild, member, channels, and Discord notification permissions before mobile dispatch.

```csharp
public async Task<BotControlExecutionContext> ResolveAsync(
    BotControlCommandRecord command,
    BotControlCommandChannelContext? channelContext,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlContextResolver.ResolveAsync(command, channelContext, cancellationToken);
```

---

### BotControlDiscordChannelResolver

**File:** `DC bot/Service/BotControl/BotControlDiscordChannelResolver.cs`

**Purpose:** Finds usable voice and response channels for mobile commands.

**Methods:**

- `SelectResponseChannelAsync` - Returns the first usable remembered, requested, or user voice channel, skipping duplicate destinations and forwarding cancellation.

```csharp
internal static Task<ulong?> SelectResponseChannelAsync(
    ulong? rememberedChannelId,
    ulong? requestedChannelId,
    ulong? voiceChannelId,
    Func<ulong, CancellationToken, Task<bool>> isUsableAsync,
    CancellationToken cancellationToken = default);
```

- `TryGetGuildAsync` — Attempts to resolve an available guild for the operation.

```csharp
internal static async Task<DiscordGuild?> TryGetGuildAsync(
    DiscordClient discordClient,
    ulong guildId,
    CancellationToken cancellationToken = default);
```

- `TryGetGuildChannelAsync` — Attempts to resolve a usable channel belonging to the selected guild.

```csharp
internal static async Task<DiscordChannel?> TryGetGuildChannelAsync(
    DiscordGuild guild,
    ulong channelId,
    CancellationToken cancellationToken = default);
```

- `IsMessageCapableChannel` — Checks whether the channel type can receive a command response.

```csharp
internal static bool IsMessageCapableChannel(DiscordChannel? channel);
```

- `IsVoiceCapableChannel` — Checks whether the channel type supports the required voice operation.

```csharp
internal static bool IsVoiceCapableChannel(DiscordChannel? channel);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, guildId, cancellationToken);
```

---

### BotControlDiscordMember

**File:** `DC bot/Service/BotControl/BotControlDiscordMember.cs`

**Purpose:** Adapts discord member data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordMember`.

**Properties:**

```csharp
public ulong Id { get; }
public bool IsBot { get; }
public string Username { get; }
public string Mention { get; }
public IDiscordVoiceState? VoiceState { get; }
```

**Methods:**

- `ToDiscordMember` — Returns the underlying DSharpPlus member represented by the adapter.

```csharp
public DiscordMember ToDiscordMember();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = botControlDiscordMember.ToDiscordMember();
```

---

### BotControlDiscordMessage

**File:** `DC bot/Service/BotControl/BotControlDiscordMessage.cs`

**Purpose:** Adapts discord message data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordMessage`.

**Properties:**

```csharp
public ulong Id { get; set; }
public string Content { get; set; }
public IDiscordChannel Channel { get; set; }
public IDiscordUser Author { get; set; }
public DateTimeOffset CreatedAt { get; set; }
public IReadOnlyList<DiscordEmbed> Embeds { get; set; }
```

**Methods:**

- `ModifyAsync` — Updates the wrapped Discord message or interaction response.

```csharp
public Task ModifyAsync(DiscordMessageBuilder builder);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public Task RespondAsync(DiscordEmbed message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botControlDiscordMessage.ModifyAsync(builder);
```

---

### BotControlDiscordResponseService

**File:** `DC bot/Service/BotControl/BotControlDiscordResponseService.cs`

**Purpose:** Publishes mobile-command outcomes to the selected Discord channel when requested.

Publishes stored result text inside a guild-localized green success or red failure embed. An unavailable embed send falls back to bounded plain text and preserves delivery success/failure reporting. HTTP payloads and stored result schemas are independent of this Discord presentation.

**Implements / inherits:** `IBotControlDiscordResponseService`.

**Methods:**

- `TrySendAsync` — Attempts Discord response delivery and reports whether delivery succeeded.

```csharp
public async Task<bool> TrySendAsync(BotControlCommandResult result, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlDiscordResponseService.TrySendAsync(result, cancellationToken);
```

---

### BotControlDiscordVoiceState

**File:** `DC bot/Service/BotControl/BotControlDiscordVoiceState.cs`

**Purpose:** Adapts discord voice state data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordVoiceState`.

**Properties:**

```csharp
public IDiscordChannel Channel { get; }
```

**Methods:**

- `ToDiscordVoiceState` — Returns the underlying DSharpPlus voice state represented by the adapter.

```csharp
public DiscordVoiceState ToDiscordVoiceState();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = botControlDiscordVoiceState.ToDiscordVoiceState();
```

---

### BotControlWorker

**File:** `DC bot/Service/BotControl/BotControlWorker.cs`

**Purpose:** Drains durable mobile commands and recovers work left running after a worker restart.

**Implements / inherits:** `IBotControlWorker`.

**Methods:**

- `RunAsync` — Recovers interrupted commands, drains pending durable commands, and waits for notifications until shutdown is requested.

```csharp
public async Task RunAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botControlWorker.RunAsync(cancellationToken);
```

---

## Factory Classes

### BotControlResultFactory

**File:** `DC bot/Service/BotControl/BotControlResultFactory.cs`

**Purpose:** Builds mobile-command outcomes and optional Discord-notification context.

**Implements / inherits:** `IBotControlResultFactory`.

**Methods:**

- `Failure` — Builds a failed operation result with error and notification context.

```csharp
public BotControlCommandResult Failure(
    BotControlCommandRecord command,
    string message,
    string errorCode,
    object? data = null,
    ulong? voiceChannelId = null,
    ulong? textChannelId = null,
    bool shouldNotifyDiscord = true);
```

- `Success` — Builds a successful operation result with response and notification context.

```csharp
public BotControlCommandResult Success(
    BotControlCommandRecord command,
    string message,
    object? data = null,
    ulong? voiceChannelId = null,
    ulong? textChannelId = null,
    bool shouldNotifyDiscord = true);
```

---

## Models and Results

### BotControlRuntimeContext

**File:** `DC bot/Service/BotControl/BotControlRuntimeContext.cs`

**Purpose:** Pairs message and voice-member adapters needed to execute a mobile music command.

**Record declaration:**

```csharp
internal sealed record BotControlRuntimeContext(IDiscordMessage Message, IDiscordMember Member);
```

---

### MobilePlayInput

**File:** `DC bot/Service/BotControl/MobilePlayInput.cs`

**Purpose:** Parses and validates a mobile play query, URL, and optional provider override.

**Record declaration:**

```csharp
public sealed record MobilePlayInput(string Query, Uri? Url, TrackSearchMode SearchMode);
```

**Methods:**

- `TryParse` — Validates supported input constraints and returns the parsed value only when valid.

```csharp
public static bool TryParse(string input, string? searchMode, ITrackSearchResolverService resolver, out MobilePlayInput? result);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = MobilePlayInput.TryParse(input, searchMode, resolver, out var result);
```

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Exceptions/Music/` — Distinguishes Lavalink failures, track-load failures, expired sessions, and busy control operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Wrapper/Discord/` — Wraps messages, users, guilds, channels, members, and voice state and handles Discord client events.
- `DC bot/Interface/Service/BotControl/` — Defines the worker, dispatch, context resolution, response, and result boundaries for API-originated commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
