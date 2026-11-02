# Discord

Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.

## Overview

- Wrapper/Discord adapts DSharpPlus objects; slash and mobile command paths can provide compatible contexts for the shared command pipeline.
- These contracts expose only the behavior the application needs. A wrapper does not replace guild-access or voice-readiness validation.

## Interfaces

### IDiscordChannel

**File:** `DC bot/Interface/Discord/IDiscordChannel.cs`

**Purpose:** Defines the discord channel boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord identifier of this object.
ulong Id { get; }
string Name { get; }
IDiscordGuild Guild { get; }
```

**Methods:**

- `SendMessageAsync` — Sends a message through the wrapped Discord channel.

```csharp
Task SendMessageAsync(DiscordEmbed embed);
```

- `SendMessageAsync` — Sends a message through the wrapped Discord channel.

```csharp
Task SendMessageAsync(string message);
```

- `ToDiscordChannel` — Returns the underlying DSharpPlus channel represented by the adapter.

```csharp
DiscordChannel ToDiscordChannel();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordChannel.SendMessageAsync(embed);
```

---

### IDiscordGuild

**File:** `DC bot/Interface/Discord/IDiscordGuild.cs`

**Purpose:** Defines the discord guild boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord identifier of this object.
ulong Id { get; }
string Name { get; }
```

**Methods:**

- `GetAllMembersAsync` — Retrieves guild members through the wrapped Discord API.

```csharp
Task<IReadOnlyCollection<IDiscordMember>> GetAllMembersAsync();
```

- `GetMemberAsync` — Resolves a guild member through the wrapped Discord API.

```csharp
Task<IDiscordMember> GetMemberAsync(ulong id);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await discordGuild.GetAllMembersAsync();
```

---

### IDiscordMember

**File:** `DC bot/Interface/Discord/IDiscordMember.cs`

**Purpose:** Defines the discord member boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord identifier of this object.
ulong Id { get; }
bool IsBot { get; }
string Username { get; }
string Mention { get; }
IDiscordVoiceState? VoiceState { get; }
```

**Methods:**

- `ToDiscordMember` — Returns the underlying DSharpPlus member represented by the adapter.

```csharp
DiscordMember ToDiscordMember();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordMember.ToDiscordMember();
```

---

### IDiscordMessage

**File:** `DC bot/Interface/Discord/IDiscordMessage.cs`

**Purpose:** Defines the discord message boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord identifier of this object.
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
Task ModifyAsync(DiscordMessageBuilder builder);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
Task RespondAsync(string message);
```

- `RespondAsync` — Sends a response through the wrapped message or interaction boundary.

```csharp
Task RespondAsync(DiscordEmbed message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await discordMessage.ModifyAsync(builder);
```

---

### IDiscordUser

**File:** `DC bot/Interface/Discord/IDiscordUser.cs`

**Purpose:** Defines the discord user boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord identifier of this object.
ulong Id { get; }
bool IsBot { get; }
string Username { get; }
string Mention { get; }
```

---

### IDiscordVoiceState

**File:** `DC bot/Interface/Discord/IDiscordVoiceState.cs`

**Purpose:** Defines the discord voice state boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The Discord channel associated with this object.
IDiscordChannel? Channel { get; }
```

**Methods:**

- `ToDiscordVoiceState` — Returns the underlying DSharpPlus voice state represented by the adapter.

```csharp
DiscordVoiceState ToDiscordVoiceState();
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordVoiceState.ToDiscordVoiceState();
```

---

## Factory Classes

### IDiscordMessageFactory

**File:** `DC bot/Interface/Discord/IDiscordMessageFactory.cs`

**Purpose:** Defines the discord message factory boundary so callers are independent of its concrete implementation.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
IDiscordMessage Create(DiscordMessage message, DiscordChannel channel, DiscordUser author, DiscordGuild? guild = null);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = discordMessageFactory.Create(message, channel, author, guild);
```

---

## Related Components

- `DC bot/Interface/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
