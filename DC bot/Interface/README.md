# Interface

Defines command and adapter boundaries used inside the bot executable.

## Overview

- ICommand is the common entry point for registered text commands. Subfolders contain core validation, Discord wrappers, and service contracts.
- Cross-process repository contracts live in DC bot.Contracts instead. Keep concrete DSharpPlus and persistence implementations out of this contract tree.

## Subfolders

### Core/

Defines command lookup, argument helpers, and user/player validation boundaries.

**Files:**

- `ICommandHelper.cs`
- `ICommandRegistry.cs`
- `IUserValidationService.cs`
- `IValidationService.cs`

**Purpose:** Command routing and command implementations depend on these abstractions; implementations live in Service/Core.

---

### Discord/

Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.

**Files:**

- `IDiscordChannel.cs`
- `IDiscordGuild.cs`
- `IDiscordMember.cs`
- `IDiscordMessage.cs`
- `IDiscordMessageFactory.cs`
- `IDiscordUser.cs`
- `IDiscordVoiceState.cs`

**Purpose:** Wrapper/Discord adapts DSharpPlus objects; slash and mobile command paths can provide compatible contexts for the shared command pipeline.

---

### Service/

Groups bot use-case contracts independently of their concrete runtime implementations.

**Files:**

- `README.md` — Folder overview and navigation to its source components.

**Purpose:** Commands, adapters, and orchestrators depend on the feature-specific interfaces below. Startup binds them to services and factories.

---

## Interfaces

### ICommand

**File:** `DC bot/Interface/ICommand.cs`

**Purpose:** Defines the command boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
// The display name.
string Name { get; }
string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Executes the registered text command using the supplied message and its response boundary.

```csharp
Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await command.ExecuteAsync(message);
```

---

### ICommandHelper

**File:** `DC bot/Interface/Core/ICommandHelper.cs`

**Purpose:** Defines the command helper boundary so callers are independent of its concrete implementation.

**Methods:**

- `TryGetArgumentAsync` — Resolves a required argument and presents usage guidance when absent.

```csharp
Task<string?> TryGetArgumentAsync(
    IDiscordMessage message,
    IResponseBuilder responseBuilder,
    ILogger logger,
    string commandName,
    CancellationToken cancellationToken = default);
```

- `TryParseSavePlaylistArguments` — Parses a playlist name and source argument while preserving quoted values.

```csharp
Task<(
    IDiscordMessage message,
    IResponseBuilder responseBuilder,
    ILogger logger,
    string commandName,
    CancellationToken cancellationToken = default);
```

- `TryValidateUserAsync` — Runs user validation and sends a localized failure response when validation fails.

```csharp
Task<UserValidationResult?> TryValidateUserAsync(
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    IDiscordMessage message,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, commandName, cancellationToken);
```

---

### ICommandRegistry

**File:** `DC bot/Interface/Core/ICommandRegistry.cs`

**Purpose:** Defines the command registry boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
IReadOnlyCollection<ICommand> Commands { get; }
```

**Methods:**

- `TryGetCommand` — Looks up a command by name without rebuilding the lazy registry snapshot.

```csharp
bool TryGetCommand(string commandName, out ICommand command);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = commandRegistry.TryGetCommand(commandName, out var command);
```

---

### IUserValidationService

**File:** `DC bot/Interface/Core/IUserValidationService.cs`

**Purpose:** Defines the user validation service boundary so callers are independent of its concrete implementation.

**Methods:**

- `ValidateUserAsync` — Checks that the requesting Discord user may perform voice-bound music operations.

```csharp
public Task<UserValidationResult> ValidateUserAsync(IDiscordMessage message, CancellationToken cancellationToken = default);
```

- `IsBotUser` — Checks whether an event user is the currently authenticated bot.

```csharp
public bool IsBotUser(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await userValidationService.ValidateUserAsync(message, cancellationToken);
```

---

### IValidationService

**File:** `DC bot/Interface/Core/IValidationService.cs`

**Purpose:** Defines the validation service boundary so callers are independent of its concrete implementation.

**Methods:**

- `ValidateConnectionAsync` — Validates Lavalink readiness using available player and remote voice-state evidence.

```csharp
public Task<ConnectionValidationResult> ValidateConnectionAsync(
    ILavalinkPlayer connection,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await validationService.ValidateConnectionAsync(connection, cancellationToken);
```

---

### IDiscordChannel

**File:** `DC bot/Interface/Discord/IDiscordChannel.cs`

**Purpose:** Defines the discord channel boundary so callers are independent of its concrete implementation.

**Properties:**

```csharp
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

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Interface/Core/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Discord/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
