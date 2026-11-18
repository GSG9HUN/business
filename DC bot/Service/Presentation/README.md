# Presentation

Builds Discord success, warning, error, and validation replies from localization keys.

## Overview

- Builds localized success, warning, validation, usage and error replies.
- Formats messages using the guild language and configured command prefix.
- Sends responses through Discord adapters with cancellation and delivery-failure handling.

## Classes

### ResponseBuilder

**File:** `DC bot/Service/Presentation/ResponseBuilder.cs`

**Purpose:** Builds localized usage, success, warning, validation, and error responses.

`SendUsageAsync` resolves `{commandName}_command_usage` using the configured prefix from `IOptions<BotSettings>`. Canonical usage errors use the same formatter. Argument errors retain their explanation and append usage; empty command context does not invent a usage destination.

**Implements / inherits:** `IResponseBuilder`.

**Methods:**

- `SendErrorAsync` — Sends a localized error response with formatting arguments.

```csharp
public async Task SendErrorAsync(
    IDiscordMessage message,
    string localizationKey,
    CancellationToken cancellationToken,
    params object[] args);
```

- `SendErrorAsync` — Sends a localized error response with formatting arguments.

```csharp
public Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args);
```

- `SendSuccessAsync` — Sends a localized success response with formatting arguments.

```csharp
public Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args);
```

- `SendSuccessAsync` — Sends a localized success response with formatting arguments.

```csharp
public async Task SendSuccessAsync(
    IDiscordMessage message,
    string localizationKey,
    CancellationToken cancellationToken,
    params object[] args);
```

- `SendUsageAsync` — Sends localized usage guidance for the requested command.

```csharp
public async Task SendUsageAsync(IDiscordMessage message, string commandName, CancellationToken cancellationToken = default);
```

- `SendValidationErrorAsync` — Sends a localized user-validation error for the command message.

```csharp
public async Task SendValidationErrorAsync(
    IDiscordMessage message,
    string errorKey,
    CancellationToken cancellationToken = default);
```

**Additional methods:** `SendWarningAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await responseBuilder.SendErrorAsync(message, localizationKey, cancellationToken, args);
```

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Localization/` — Defines translation lookup, formatting, and guild language selection.
- `DC bot/Interface/Service/Presentation/` — Defines localized success, warning, error, and validation responses.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
