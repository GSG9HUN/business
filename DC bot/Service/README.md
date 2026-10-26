# Service

Groups process lifecycle, localization, command orchestration, music, reactions, and slash execution.

## Overview

- BotService runs the Discord client; LocalizationService handles translations and guild language files. Feature folders own the larger workflows.
- Services receive dependencies from Startup. Persistence implementations remain in DC bot.Persistence, and SDK-specific object adaptation remains in Wrapper.

## Subfolders

### BotControl/

Executes API-originated commands from the durable PostgreSQL command bridge.

**Files:**

- `BotControlCommandDispatcher.cs`
- `BotControlContextResolver.cs`
- `BotControlDiscordChannelResolver.cs`
- `BotControlDiscordMember.cs`
- `BotControlDiscordMessage.cs`
- `BotControlDiscordResponseService.cs`
- `BotControlDiscordVoiceState.cs`
- `BotControlResultFactory.cs`
- `BotControlRuntimeContext.cs`
- `BotControlWorker.cs`
- `MobilePlayInput.cs`

**Purpose:** BotControlWorker claims work, resolves Discord context, dispatches allowed commands, and persists result or failure metadata. Adapter values make mobile requests compatible with existing bot boundaries.

---

### Core/

Handles text command routing, registry snapshots, argument parsing, and user/player readiness validation.

**Files:**

- `CommandHandlerService.cs`
- `CommandRegistry.cs`
- `CommandRegistrySnapshot.cs`
- `CommandValidationService.cs`
- `ValidationService.cs`

**Purpose:** CommandHandlerService dispatches registered ICommand values. CommandRegistry materializes lookup state; CommandValidationService parses command payloads. ValidationService checks voice context and player readiness.

---

### Music/

Coordinates music requests through a facade and specialized playback components.

**Files:**

- `LavaLinkService.cs`
- `TrackSearchResolverService.cs`

**Purpose:** LavaLinkService connects commands to request, lifecycle, control, queue, and track-playback services. TrackSearchResolverService interprets source prefixes and provider URLs.

---

### Presentation/

Builds Discord success, warning, error, and validation replies from localization keys.

**Files:**

- `ResponseBuilder.cs`

**Purpose:** ResponseBuilder retrieves the guild language, formats arguments, and sends through IDiscordMessage.

---

### ReactionHandler/

Connects now-playing messages and Discord reactions to music controls.

**Files:**

- `ReactionActionDispatcher.cs`
- `ReactionContext.cs`
- `ReactionContextFactory.cs`
- `ReactionControlEmojis.cs`
- `ReactionControlMessageService.cs`
- `ReactionHandlerService.cs`

**Purpose:** The handler registers reaction processing; context factories validate the reacting user and message; the dispatcher maps known emoji to control operations.

---

### SlashCommands/

Reuses registered text commands for Discord slash interactions.

**Files:**

- `SlashCommandExecutor.cs`

**Purpose:** SlashCommandExecutor resolves the named command, validates guild context, handles deferral, and dispatches through an interaction-backed message wrapper.

---

## Classes

### BotService

**File:** `DC bot/Service/BotService.cs`

**Purpose:** Starts the Discord client and keeps the application alive until lifetime cancellation.

**Methods:**

- `StartAsync` — Connects Discord and optionally waits until application-lifetime cancellation.

```csharp
public async Task StartAsync(bool isTestEnvironment = false, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await botService.StartAsync(isTestEnvironment, cancellationToken);
```

---

### LocalizationService

**File:** `DC bot/Service/LocalizationService.cs`

**Purpose:** Loads language dictionaries and persists guild language preferences through the file-system boundary.

**Implements / inherits:** `ILocalizationService`.

**Methods:**

- `Get` — Resolves a localized value for the supplied key and formatting arguments.

```csharp
public string Get(ulong guildId, string key, params object[] args);
```

- `Get` — Resolves a localized value for the supplied key and formatting arguments.

```csharp
public string Get(string key, params object[] args);
```

- `LoadLanguage` — Loads the guild's saved language preference and dictionary.

```csharp
public void LoadLanguage(ulong guildId);
```

- `SaveLanguage` — Persists the guild's language preference and updates cached translations.

```csharp
public void SaveLanguage(ulong guildId, string language);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = localizationService.Get(guildId, key, args);
```

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Service/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Core/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Music/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/Presentation/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/ReactionHandler/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Service/SlashCommands/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/IO/` — Implements the injected file-system boundary for translation and guild preference data.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Localization/` — Represents failures while loading or resolving translation resources.
- `DC bot/Interface/Service/IO/` — Abstracts the file operations required by localization and guild preference storage.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
