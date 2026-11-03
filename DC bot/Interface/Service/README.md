# Service

Groups bot use-case contracts independently of their concrete runtime implementations.

## Overview

- Commands, adapters, and orchestrators depend on the feature-specific interfaces below. Startup binds them to services and factories.
- Storage interfaces shared with the API are in DC bot.Contracts. Avoid leaking EF contexts into this tree.

## Subfolders

### BotControl/

Defines the worker, dispatch, context resolution, response, and result boundaries for API-originated commands.

**Files:**

- `IBotControlCommandDispatcher.cs`
- `IBotControlContextResolver.cs`
- `IBotControlDiscordResponseService.cs`
- `IBotControlResultFactory.cs`
- `IBotControlWorker.cs`

**Purpose:** BotControlWorker claims durable commands and resolves enough Discord context to reuse the bot's command and music services.

---

### IO/

Abstracts the file operations required by localization and guild preference storage.

**Files:**

- `IFileSystem.cs`

**Purpose:** PhysicalFileSystem supplies the production implementation; callers receive the contract through DI.

---

### Localization/

Defines translation lookup, formatting, and guild language selection.

**Files:**

- `ILocalizationService.cs`

**Purpose:** LocalizationService loads translation resources and uses the file-system abstraction for persisted guild preferences.

---

### Music/

Exposes the top-level music operations used by commands and adapters.

**Files:**

- `ILavaLinkService.cs`

**Purpose:** ILavaLinkService delegates to specialized request, connection, control, queue, and playback services.

---

### Presentation/

Defines localized success, warning, error, and validation responses.

**Files:**

- `IResponseBuilder.cs`

**Purpose:** ResponseBuilder formats translated text into Discord responses and supports caller cancellation where the underlying boundary allows it.

---

### SlashCommands/

Connects DSharpPlus interactions to the shared text command pipeline.

**Files:**

- `ISlashCommandExecutor.cs`
- `ISlashInteractionContext.cs`
- `ISlashInteractionContextFactory.cs`
- `SlashCommandExecutionRequest.cs`

**Purpose:** Factories create interaction contexts; execution requests describe the text command and argument payload to dispatch.

---

## Related Components

- `DC bot/Interface/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Interface/Service/BotControl/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/IO/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Localization/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Music/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/Presentation/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Interface/Service/SlashCommands/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
