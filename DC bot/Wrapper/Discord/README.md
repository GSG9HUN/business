# Discord

Wraps messages, users, guilds, channels, members, and voice state and handles Discord client events.

## Overview

- DiscordClientEventHandler routes connection and voice events to status persistence and playback recovery. Separate loggers keep event diagnostics focused.
- Voice event payloads and cached member/channel values can be incomplete. Gateway reconnection and voice transport recovery are separate lifecycle concerns.

## Classes

### DiscordChannelWrapper

**File:** `DC bot/Wrapper/Discord/DiscordChannelWrapper.cs`

**Purpose:** Adapts discord channel data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordChannel`.

**Properties:**

```csharp
public ulong Id { get; }
public string Name { get; }
public IDiscordGuild Guild { get; }
```

**Methods:**

- `SendMessageAsync` — Sends a message through the wrapped Discord channel.

```csharp
public async Task SendMessageAsync(DiscordEmbed embed);
```

- `SendMessageAsync` — Sends a message through the wrapped Discord channel.

```csharp
public async Task SendMessageAsync(string message);
```

- `ToDiscordChannel` — Returns the underlying DSharpPlus channel represented by the adapter.

```csharp
public DiscordChannel ToDiscordChannel();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordChannelWrapper.SendMessageAsync(embed);
```

---

### DiscordClientEventHandler

**File:** `DC bot/Wrapper/Discord/DiscordClientEventHandler.cs`

**Purpose:** Isolates event failures while maintaining gateway recovery and persisted bot voice state.

**Methods:**

- `OnClientReady` — Processes the Discord client ready event inside the common event-failure boundary.

```csharp
public async Task OnClientReady(DiscordClient sender, SessionCreatedEventArgs? e);
```

- `OnGuildAvailable` — Processes the Discord guild available event inside the common event-failure boundary.

```csharp
public async Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e);
```

- `OnSessionResumed` — Processes the Discord session resumed event inside the common event-failure boundary.

```csharp
public Task OnSessionResumed(DiscordClient sender, SessionResumedEventArgs e);
```

- `OnSocketClosed` — Processes the Discord socket closed event inside the common event-failure boundary.

```csharp
public Task OnSocketClosed(DiscordClient sender, SocketClosedEventArgs e);
```

- `OnSocketOpened` — Processes the Discord socket opened event inside the common event-failure boundary.

```csharp
public Task OnSocketOpened(DiscordClient sender, SocketOpenedEventArgs e);
```

- `OnUnknownEvent` — Processes the Discord unknown event event inside the common event-failure boundary.

```csharp
public Task OnUnknownEvent(DiscordClient sender, UnknownEventArgs e);
```

**Additional methods:** `OnZombied`, `OnVoiceStateUpdated`, `OnVoiceServerUpdated`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordClientEventHandler.OnClientReady(sender, e);
```

---

### DiscordConnectionEventLogger

**File:** `DC bot/Wrapper/Discord/DiscordConnectionEventLogger.cs`

**Purpose:** Records gateway and socket diagnostics with bounded unknown-event payloads.

**Methods:**

- `LogClientReady` — Records client ready diagnostics using available Discord event state.

```csharp
internal void LogClientReady(SessionCreatedEventArgs? e);
```

- `LogSessionResumed` — Records session resumed diagnostics using available Discord event state.

```csharp
internal void LogSessionResumed(SessionResumedEventArgs e);
```

- `LogSocketClosed` — Records socket closed diagnostics using available Discord event state.

```csharp
internal void LogSocketClosed(SocketClosedEventArgs e);
```

- `LogSocketOpened` — Records socket opened diagnostics using available Discord event state.

```csharp
internal void LogSocketOpened();
```

- `LogUnknownEvent` — Records unknown event diagnostics using available Discord event state.

```csharp
internal void LogUnknownEvent(UnknownEventArgs e);
```

- `LogZombied` — Records zombied diagnostics using available Discord event state.

```csharp
internal void LogZombied(ZombiedEventArgs e);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
discordConnectionEventLogger.LogClientReady(e);
```

---

### DiscordGuildWrapper

**File:** `DC bot/Wrapper/Discord/DiscordGuildWrapper.cs`

**Purpose:** Adapts discord guild data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordGuild`.

**Properties:**

```csharp
public ulong Id { get; }
public string Name { get; }
```

**Methods:**

- `GetAllMembersAsync` — Retrieves guild members through the wrapped Discord API.

```csharp
public async Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
```

- `GetMemberAsync` — Resolves a guild member through the wrapped Discord API.

```csharp
public async Task<IDiscordMember> GetMemberAsync(ulong id);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await discordGuildWrapper.GetAllMembersAsync();
```

---

### DiscordMemberWrapper

**File:** `DC bot/Wrapper/Discord/DiscordMemberWrapper.cs`

**Purpose:** Adapts discord member data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordMember`.

**Properties:**

```csharp
public ulong Id { get; }
public bool IsBot { get; }
public string Username { get; }
public string Mention { get; }
public IDiscordVoiceState VoiceState { get; }
```

**Methods:**

- `ToDiscordMember` — Returns the underlying DSharpPlus member represented by the adapter.

```csharp
public DiscordMember ToDiscordMember();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordMemberWrapper.ToDiscordMember();
```

---

### DiscordMessageWrapper

**File:** `DC bot/Wrapper/Discord/DiscordMessageWrapper.cs`

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
public async Task ModifyAsync(DiscordMessageBuilder builder);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public async Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
public async Task RespondAsync(DiscordEmbed embed);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordMessageWrapper.ModifyAsync(builder);
```

---

### DiscordUserWrapper

**File:** `DC bot/Wrapper/Discord/DiscordUserWrapper.cs`

**Purpose:** Adapts discord user data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordUser`.

**Properties:**

```csharp
public ulong Id { get; }
public bool IsBot { get; }
public string Username { get; }
public string Mention { get; }
```

---

### DiscordVoiceEventLogger

**File:** `DC bot/Wrapper/Discord/DiscordVoiceEventLogger.cs`

**Purpose:** Records voice changes while tolerating missing event entities.

**Methods:**

- `LogVoiceServerUpdated` — Records voice server updated diagnostics using available Discord event state.

```csharp
internal void LogVoiceServerUpdated(VoiceServerUpdatedEventArgs e);
```

- `LogVoiceStateUpdated` — Records voice state updated diagnostics using available Discord event state.

```csharp
internal void LogVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
discordVoiceEventLogger.LogVoiceServerUpdated(e);
```

---

### DiscordVoiceStateWrapper

**File:** `DC bot/Wrapper/Discord/DiscordVoiceStateWrapper.cs`

**Purpose:** Adapts discord voice state data and operations to the bot's Discord boundary.

**Implements / inherits:** `IDiscordVoiceState`.

**Properties:**

```csharp
public IDiscordChannel? Channel
{
    get
    {
        if (discordVoiceState?.ChannelId is null or 0)
        {
            return null;
        }

        var channel = discordVoiceState.GetChannelAsync()
            .GetAwaiter()
            .GetResult();
        return channel is null ? null : new DiscordChannelWrapper(channel);
    }
}
```

**Methods:**

- `ToDiscordVoiceState` — Returns the underlying DSharpPlus voice state represented by the adapter.

```csharp
public DiscordVoiceState ToDiscordVoiceState();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordVoiceStateWrapper.ToDiscordVoiceState();
```

---

## Factory Classes

### DiscordClientFactory

**File:** `DC bot/Wrapper/Discord/DiscordClientFactory.cs`

**Purpose:** Creates the Discord client with configured credentials, intents, and event handling.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public static DiscordClient Create(BotSettings settings);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = DiscordClientFactory.Create(settings);
```

---

## Related Components

- `DC bot/Wrapper/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Localization/` — Defines translation lookup, formatting, and guild language selection.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
