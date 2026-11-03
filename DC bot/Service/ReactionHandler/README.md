# ReactionHandler

Connects now-playing messages and Discord reactions to music controls.

## Overview

- The handler registers reaction processing; context factories validate the reacting user and message; the dispatcher maps known emoji to control operations.
- Keep reaction identity checks and response delivery distinct from playback state transitions. All control paths must respect the same guild session ownership used by commands.

## Classes

### ReactionActionDispatcher

**File:** `DC bot/Service/ReactionHandler/ReactionActionDispatcher.cs`

**Purpose:** Maps supported reaction emojis to music controls and localized responses.

**Methods:**

- `DispatchAddedAsync` — Dispatches the added operation to its resolved service handler.

```csharp
public async Task DispatchAddedAsync(
    string emojiName,
    IDiscordMessage message,
    IDiscordMember member,
    CancellationToken cancellationToken = default);
```

- `DispatchRemovedAsync` — Dispatches the removed operation to its resolved service handler.

```csharp
public async Task DispatchRemovedAsync(
    string emojiName,
    IDiscordMessage message,
    IDiscordMember member,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await reactionActionDispatcher.DispatchAddedAsync(emojiName, message, member, cancellationToken);
```

---

### ReactionControlEmojis

**File:** `DC bot/Service/ReactionHandler/ReactionControlEmojis.cs`

**Purpose:** Defines the emojis recognized by reaction-based music controls.

**Methods:**

- `Normalize` — Normalizes supported input before provider-specific validation or dispatch.

```csharp
public static string Normalize(string emojiName);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = ReactionControlEmojis.Normalize(emojiName);
```

---

### ReactionControlMessageService

**File:** `DC bot/Service/ReactionHandler/ReactionControlMessageService.cs`

**Purpose:** Sends track control messages and attaches supported control reactions.

**Methods:**

- `SendAsync` — Delivers the response or control message through the selected Discord presentation boundary.

```csharp
public async Task SendAsync(
    IDiscordChannel textChannel,
    DiscordClient client,
    DiscordEmbed embed,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await reactionControlMessageService.SendAsync(textChannel, client, embed, cancellationToken);
```

---

### ReactionHandlerService

**File:** `DC bot/Service/ReactionHandler/ReactionHandlerService.cs`

**Purpose:** Registers reaction events and routes supported controls for bot control messages.

**Implements / inherits:** `IEventHandler<MessageReactionAddedEventArgs>`, `IEventHandler<MessageReactionRemovedEventArgs>`.

**Methods:**

- `ExecuteOnReactionAddedAsync` — Processes execute on reaction added within the reaction handler service boundary using the supplied context.

```csharp
internal async Task ExecuteOnReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member);
```

- `ExecuteOnReactionRemovedAsync` — Processes execute on reaction removed within the reaction handler service boundary using the supplied context.

```csharp
internal async Task ExecuteOnReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member);
```

- `HandleEventAsync` — Dispatches the added or removed reaction only while the reaction handler is registered.

```csharp
public Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs eventArgs);
```

- `HandleEventAsync` — Dispatches the added or removed reaction only while the reaction handler is registered.

```csharp
public Task HandleEventAsync(DiscordClient sender, MessageReactionRemovedEventArgs eventArgs);
```

- `HandleReactionAddedAsync` — Routes the reaction added operation through its resolved context and records the outcome.

```csharp
internal Task HandleReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member);
```

- `HandleReactionRemovedAsync` — Routes the reaction removed operation through its resolved context and records the outcome.

```csharp
internal Task HandleReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member);
```

**Additional methods:** `RegisterHandler`, `UnregisterHandler`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await reactionHandlerService.ExecuteOnReactionAddedAsync(emojiName, message, member);
```

---

## Factory Classes

### ReactionContextFactory

**File:** `DC bot/Service/ReactionHandler/ReactionContextFactory.cs`

**Purpose:** Resolves the Discord entities required for a supported music-control reaction.

**Methods:**

- `CreateAsync` — Resolves and constructs the context required by the requested Discord operation.

```csharp
public async Task<ReactionContext> CreateAsync(
    DiscordMessage message,
    DiscordUser user,
    DiscordChannel channel,
    DiscordGuild? guild = null,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await reactionContextFactory.CreateAsync(message, user, channel, guild, cancellationToken);
```

---

## Models and Results

### ReactionContext

**File:** `DC bot/Service/ReactionHandler/ReactionContext.cs`

**Purpose:** Carries the message, guild, member, and channel used by a music-control reaction.

**Record declaration:**

```csharp
public sealed record ReactionContext(IDiscordMember Member, IDiscordMessage Message, ulong GuildId);
```

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Exceptions/` — Defines the common exception boundary and feature-specific failure categories.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Messaging/` — Represents Discord response delivery failures at the presentation boundary.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
