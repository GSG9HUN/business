# Presentation

Builds Discord success, warning, error, and validation replies from localization keys.

## Overview

- Favorite response titles resolve only the registered `favorite` command; help uses the generic no-alias guidance.

- Usage lookup resolves command casing through the existing command registry before selecting the catalog key. Catalog usage includes separate text and verified slash syntax, group routes and confirmation options.

- Builds localized, status-colored embeds for success, warning, validation, usage and error replies.
- Formats messages using the guild language and configured command prefix.
- Sends responses through Discord adapters with cancellation and delivery-failure handling.

## Classes

### ResponseBuilder

**File:** `DC bot/Service/Presentation/ResponseBuilder.cs`

**Purpose:** Builds localized usage, success, warning, validation, and error responses.

`CreateEmbed` builds bounded presentation for channel notifications, slash guards, confirmations and interaction menus. It accepts an optional guild ID, status color and localized title/footer keys; omitted styling uses an Azure informational embed. Now-playing retains artwork and metadata. `SendHistoryAsync` formats batch status, track count and UTC creation time through the shared embed and text-fallback pipeline. Queue listing uses the common numbered pagination layout. Command-context titles use `CommandNames`.

**Presentation:**

| Response | Color | Localized labels |
| --- | --- | --- |
| Success | Green | Result-specific title and success icon |
| Warning | Gold | Result-specific title and warning icon |
| Error | Red | Result-specific title and error icon |
| User condition / rejected request | Gold | Actionable title and warning icon |
| System failure | Red | Error title and icon |
| Information / read-only state | Azure | Context title and information icon |
| Usage | Azure | Usage title and prefix-aware help footer |

Short status replies use localized titles such as voice-channel requirements, full/empty queues and 24/7 enabled/disabled. Generic playback results resolve their title from the dispatched command name. Status sends omit repeated generic footers; usage guidance and explicit custom footers remain available. Empty queue replies append configured-prefix and slash play guidance.

**Validation Checks:**

- Titles are bounded to 256 characters, descriptions to 4096 and footers to 2048; combined text stays within 6000 characters.
- Truncation adds an ellipsis without cutting a UTF-16 surrogate pair.
- Failed embed delivery falls back to plain text bounded to 2000 characters. Text fallback failures are logged.
- Caller cancellation propagates through supported waits and does not trigger the central fallback.
- Native Discord wrappers handle their own embed fallback; the response builder does not send a duplicate after the wrapper completes.
- Guild language, formatting arguments, configured-prefix usage and syntax-error guidance apply to embeds as well as fallback text.

`SendUsageAsync` resolves `{commandName}_command_usage` using the configured prefix from `IOptions<BotSettings>`. Canonical usage errors use the same formatter. Argument errors retain their explanation and append usage; empty command context does not invent a usage destination.

`CreateValidationEmbed` classifies known user conditions, capacity limits, unsupported operations and unconfirmed outcomes as warnings. System failures and unrecognized failure keys remain errors. `SendValidationErrorAsync` uses that same classification and preserves usage guidance.

`SendInfoAsync` presents read-only state without claiming a change. `SendPlaybackResultAsync` honors `IsInformational` and `HasWarning`; warning metadata takes precedence over information, while completed changes remain successes.

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

**Purpose:** Presents queue, playlist, help, favorite, lyrics and custom-preset pages through one expiring Discord message. `SendHelpAsync` accepts typed `HelpResponse` state and delegates layout/navigation to `IHelpResponseBuilder`; ordinary lists retain their embed and Previous/Next layout.

**Methods:**

`FromItems` creates snapshot pages with at most ten rows and a 3,500-character description budget. `FromText` splits continuous text at line boundaries when possible and retains UTF-16 surrogate pairs. `SendAsync` sends the initial page; `OnComponentInteractionAsync` processes navigation.

`PaginationRequest` supplies a lazy page callback with optional preloaded initial content. Help has separate typed navigation instead of renderer/navigation delegates on generic page models. Favorites and custom presets query scoped totals and ten records per button press; loaded queue, playlist and help lists retain their original snapshot. Lyrics retain the original provider response.

**Usage:**

```csharp
await pagination.SendAsync(message, DiscordPaginationService.FromItems(title, rows));
```

**Validation Checks:**

- Ordinary Previous/Next edits the existing embed; help edits a single V2 response through category, page and detail transitions. A footer shows registry counts or category page totals.
- Ordinary one-page lists omit buttons; boundary buttons are disabled. Help home/category/detail controls follow their view, with no pagination on one-page categories. Versioned custom IDs reject stale view actions, and select values must belong to the displayed category/page.
- Failed initial component sends retire the panel and retry its requested page as plain text bounded to 2000 characters, including title and page/item totals. Cancellation propagates without retrying delivery. A failed custom-view edit retires its session because remote acceptance is uncertain; ordinary list error behavior is retained. Expiry cleanup bounds both gate waiting and editing to ten seconds. An in-flight edit that completes after expiry removes its controls before releasing ownership.
- Controls bind the requesting user, guild, channel and sent message. Rejections are localized and ephemeral.
- A new multi-page result replaces the same user's previous panel in that guild; at most 512 panels retain callbacks.
- Controls expire after five minutes, and expired/replaced messages have their buttons disabled.
- Concurrent page edits are serialized, interactions are acknowledged before database reads and page queries have a ten-second cancellation deadline.
- Provider/read failures retain the displayed page and report a localized ephemeral error. Paging never mutates playback.
- Async disposal cancels panel lifetimes.

---

### HelpResponseBuilder

**File:** `HelpResponseBuilder.cs`

**Purpose:** Captures registered command metadata, resolves localized category/detail content and examples, validates typed navigation and builds bounded native Components V2 containers. It never sends or edits Discord messages.

**Methods:** `Create` opens home or case-insensitive command details; `Navigate` returns the next valid immutable view or null; `BuildMessage` renders versioned controls and omits them after expiry.

**Validation Checks:** Only displayed category/page selections are accepted. Seven-command pages retain the return page independently of the details page. Canonical names use `CommandNames`; all user-facing help labels and example templates use `LocalizationKeys` and the bilingual catalogs.

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Interface/Discord/` — Abstracts the Discord message, channel, guild, member, user, and voice-state values consumed by commands.
- `DC bot/Interface/Service/Localization/` — Defines translation lookup, formatting, and guild language selection.
- `DC bot/Interface/Service/Presentation/` — Defines localized success, warning, error, and validation responses.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
