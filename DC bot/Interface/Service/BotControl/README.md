# BotControl

Defines the worker, dispatch, context resolution, response, and result boundaries for API-originated commands.

## Overview

- BotControlWorker claims durable commands and resolves enough Discord context to reuse the bot's command and music services.
- Claim ownership, allowed command types, and response persistence are separate concerns. Mobile requests must not bypass validation merely because they came through the database.

## Subfolders

### Models/

Carries resolved Discord execution context and normalized command results within the bot.

**Files:**

- `BotControlCommandResult.cs`
- `BotControlExecutionContext.cs`

**Purpose:** The dispatcher and response service use these values to execute an accepted command and persist its outcome.

---

## Interfaces

### IBotControlCommandDispatcher

**File:** `DC bot/Interface/Service/BotControl/IBotControlCommandDispatcher.cs`

**Purpose:** Defines the bot control command dispatcher boundary so callers are independent of its concrete implementation.

**Methods:**

- `DispatchAsync` — Dispatches the operation to its resolved service handler.

```csharp
Task<BotControlCommandResult> DispatchAsync(BotControlCommandRecord command, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlCommandDispatcher.DispatchAsync(command, cancellationToken);
```

---

### IBotControlContextResolver

**File:** `DC bot/Interface/Service/BotControl/IBotControlContextResolver.cs`

**Purpose:** Defines the bot control context resolver boundary so callers are independent of its concrete implementation.

**Methods:**

- `ResolveAsync` — Resolves the command's guild, member, channels, and notification permissions before mobile dispatch.

```csharp
Task<BotControlExecutionContext> ResolveAsync(
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

### IBotControlDiscordResponseService

**File:** `DC bot/Interface/Service/BotControl/IBotControlDiscordResponseService.cs`

**Purpose:** Defines the bot control discord response service boundary so callers are independent of its concrete implementation.

**Methods:**

- `TrySendAsync` — Attempts Discord response delivery and reports whether delivery succeeded.

```csharp
Task<bool> TrySendAsync(BotControlCommandResult result, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = await botControlDiscordResponseService.TrySendAsync(result, cancellationToken);
```

---

### IBotControlWorker

**File:** `DC bot/Interface/Service/BotControl/IBotControlWorker.cs`

**Purpose:** Defines the bot control worker boundary so callers are independent of its concrete implementation.

**Methods:**

- `RunAsync` — Processes durable mobile commands and their notification lifecycle until shutdown is requested.

```csharp
Task RunAsync(CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botControlWorker.RunAsync(cancellationToken);
```

---

## Factory Classes

### IBotControlResultFactory

**File:** `DC bot/Interface/Service/BotControl/IBotControlResultFactory.cs`

**Purpose:** Defines the bot control result factory boundary so callers are independent of its concrete implementation.

**Methods:**

- `Failure` — Builds a failed operation result with error and notification context.

```csharp
BotControlCommandResult Failure(
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
BotControlCommandResult Success(
    BotControlCommandRecord command,
    string message,
    object? data = null,
    ulong? voiceChannelId = null,
    ulong? textChannelId = null,
    bool shouldNotifyDiscord = true);
```

---

## Related Components

- `DC bot/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/BotControl/Models/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/BotControl/Models/` — Carries resolved Discord execution context and normalized command results within the bot.
- `DC bot.Contracts/BotControl/` — Defines command type identifiers, realtime event names, and optional execution-channel context shared by the API and bot.
- `DC bot.Contracts/Interface/Service/Persistence/Models/BotControlCommand/` — Defines shared records and state values for bot control command operations.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
