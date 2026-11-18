# Presentation

Defines localized success, warning, error, validation and usage responses.

## Overview

- ResponseBuilder presents translated messages as status-colored Discord embeds, with bounded plain-text fallback on delivery failure.
- Caller cancellation is supported where the underlying boundary allows it.
- Keep localization arguments separate from cancellation tokens. Playback side effects belong to music services, not response formatting.

## Subfolders

### Models/

Carries read-only pagination requests and formatted page content.

**Files:**

- `PaginationPage.cs`
- `PaginationRequest.cs`

**Purpose:** Separates page values and cancellable loaders from Discord interaction ownership.

---

## Interfaces

### IResponseBuilder

**File:** `DC bot/Interface/Service/Presentation/IResponseBuilder.cs`

**Purpose:** Defines the response builder boundary so callers are independent of its concrete implementation.

**Methods:**

```csharp
DiscordEmbed CreateEmbed(
    ulong? guildId,
    string description,
    DiscordColor? color = null,
    string? titleKey = null,
    string? footerKey = null);
```

Creates presentation for callers that own a channel send, interaction response or component message. The caller retains delivery, acknowledgement and cancellation ownership.

**Methods:**

- `SendErrorAsync` — Sends a localized error response with formatting arguments.

```csharp
Task SendErrorAsync(IDiscordMessage message, string localizationKey, CancellationToken cancellationToken, params object[] args);
```

- `SendErrorAsync` — Sends a localized error response with formatting arguments.

```csharp
Task SendErrorAsync(IDiscordMessage message, string localizationKey, params object[] args);
```

- `SendSuccessAsync` — Sends a localized success response with formatting arguments.

```csharp
Task SendSuccessAsync(IDiscordMessage message, string localizationKey, params object[] args);
```

- `SendSuccessAsync` — Sends a localized success response with formatting arguments.

```csharp
Task SendSuccessAsync(IDiscordMessage message, string localizationKey, CancellationToken cancellationToken, params object[] args);
```

- `SendUsageAsync` — Sends localized usage guidance for the requested command.

```csharp
Task SendUsageAsync(IDiscordMessage message, string commandName, CancellationToken cancellationToken = default);
```

- `SendValidationErrorAsync` — Sends a localized user-validation error for the command message.

```csharp
Task SendValidationErrorAsync(IDiscordMessage message, string errorKey, CancellationToken cancellationToken = default);
```

**Additional methods:** `SendWarningAsync`. See the source and its XML contracts for the complete parameter lists.

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await responseBuilder.SendErrorAsync(message, localizationKey, cancellationToken, args);
```

---

## Related Components

- `DC bot/Interface/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
