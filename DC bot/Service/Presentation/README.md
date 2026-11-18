# Presentation

Builds Discord success, warning, error, and validation replies from localization keys.

## Overview

- Builds localized, status-colored embeds for success, warning, validation, usage and error replies.
- Formats messages using the guild language and configured command prefix.
- Sends responses through Discord adapters with cancellation and delivery-failure handling.

## Classes

### ResponseBuilder

**File:** `DC bot/Service/Presentation/ResponseBuilder.cs`

**Purpose:** Builds localized usage, success, warning, validation, and error responses.

`CreateEmbed` builds bounded presentation for channel notifications, slash guards, confirmations and interaction menus. It accepts an optional guild ID, status color and localized title/footer keys; omitted styling uses an Azure informational embed. Custom now-playing and history embeds retain their structured fields. Queue listing uses the common numbered pagination layout.

**Presentation:**

| Response | Color | Localized labels |
| --- | --- | --- |
| Success | Green | Success title and footer |
| Warning | Gold | Warning title and footer |
| Error | Red | Error title and footer |
| Validation error | Red | Error title and validation footer |
| Usage | Azure | Usage title and prefix-aware help footer |

**Validation Checks:**

- Titles are bounded to 256 characters, descriptions to 4096 and footers to 2048; combined text stays within 6000 characters.
- Truncation adds an ellipsis without cutting a UTF-16 surrogate pair.
- Failed embed delivery falls back to plain text bounded to 2000 characters. Text fallback failures are logged.
- Caller cancellation propagates through supported waits and does not trigger the central fallback.
- Native Discord wrappers handle their own embed fallback; the response builder does not send a duplicate after the wrapper completes.
- Guild language, formatting arguments, configured-prefix usage and syntax-error guidance apply to embeds as well as fallback text.

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

### DiscordPaginationService

**File:** `DiscordPaginationService.cs`

**Purpose:** Presents queue, playlist, help, favorite, lyrics and custom-preset pages through one expiring Discord message.

**Methods:**

`FromItems` creates snapshot pages with at most ten rows and a 3,500-character description budget. `FromText` splits continuous text at line boundaries when possible and retains UTF-16 surrogate pairs. `SendAsync` sends the initial page; `OnComponentInteractionAsync` processes navigation.

`PaginationRequest` can instead supply a lazy page callback with preloaded initial content. Favorites and custom presets query scoped totals and ten records per button press; loaded queue, playlist and help lists retain their original snapshot. Lyrics retain the original provider response.

**Usage:**

```csharp
await pagination.SendAsync(message, DiscordPaginationService.FromItems(title, rows));
```

**Validation Checks:**

- Previous/Next edits the existing embed; a footer shows page/total and item count for lists.
- One-page results omit buttons; boundary buttons are disabled.
- Failed initial component sends retire the panel and retry its requested page as plain text bounded to 2000 characters, including title and page/item totals. Cancellation propagates without retrying delivery.
- Controls bind the requesting user, guild, channel and sent message. Rejections are localized and ephemeral.
- A new multi-page result replaces the same user's previous panel in that guild; at most 512 panels retain callbacks.
- Controls expire after five minutes, and expired/replaced messages have their buttons disabled.
- Concurrent page edits are serialized, interactions are acknowledged before database reads and page queries have a ten-second cancellation deadline.
- Provider/read failures retain the displayed page and report a localized ephemeral error. Paging never mutates playback.
- Async disposal cancels panel lifetimes.

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Localization/` — Defines translation lookup, formatting, and guild language selection.
- `DC bot/Interface/Service/Presentation/` — Defines localized success, warning, error, and validation responses.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
