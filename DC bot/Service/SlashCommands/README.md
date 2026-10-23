# SlashCommands

Reuses registered text commands for Discord slash interactions.

## Overview

- SlashCommandExecutor resolves the named command, validates guild context, handles deferral, and dispatches through an interaction-backed message wrapper.
- Keep interaction response state in the slash adapter. Commands remain responsible for their ordinary validation, services, and localized outcomes.

## Classes

### SlashCommandExecutor

**File:** `DC bot/Service/SlashCommands/SlashCommandExecutor.cs`

**Purpose:** Adapts slash interactions to registered commands and completes deferred interaction responses.

**Implements / inherits:** `ISlashCommandExecutor`.

**Methods:**

- `ExecuteAsync` — Executes the command request through the registered command pipeline and completes its response.

```csharp
public Task ExecuteAsync(SlashCommandExecutionRequest request, CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await slashCommandExecutor.ExecuteAsync(request, cancellationToken);
```

---

## Related Components

- `DC bot/Service/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Exceptions/` — Defines the common exception boundary and feature-specific failure categories.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
