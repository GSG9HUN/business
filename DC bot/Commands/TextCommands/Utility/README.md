# Utility

Exposes help, language selection, latency checks, and user tagging through text commands.

## Overview

- CommandHandlerService selects an ICommand by name. The command parses input, validates the required context, calls injected services, and sends a localized result.
- Keep user input and response presentation here. Playback ownership, track transport, and database transitions belong to the services reached by the command.

## Classes

### HelpCommand

**File:** `DC bot/Commands/TextCommands/Utility/HelpCommand.cs`

**Purpose:** Handles the help text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `help`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await helpCommand.ExecuteAsync(message);
```

---

### LanguageCommand

**File:** `DC bot/Commands/TextCommands/Utility/LanguageCommand.cs`

**Purpose:** Handles the language text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `language`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await languageCommand.ExecuteAsync(message);
```

---

### PingCommand

**File:** `DC bot/Commands/TextCommands/Utility/PingCommand.cs`

**Purpose:** Handles the ping text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `ping`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await pingCommand.ExecuteAsync(message);
```

---

### TagCommand

**File:** `DC bot/Commands/TextCommands/Utility/TagCommand.cs`

**Purpose:** Handles the tag text command through validation, service, and localized response boundaries.

**Implements / inherits:** `ICommand`.

**Command identifier:** `tag`. Message routing applies the configured prefix.

**Properties:**

```csharp
public string Name { get; }
public string Description { get; }
```

**Methods:**

- `ExecuteAsync` — Validates the message context, performs this command's action, and sends its localized outcome.

```csharp
public async Task ExecuteAsync(IDiscordMessage message);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await tagCommand.ExecuteAsync(message);
```

---

## Related Components

- `DC bot/Commands/TextCommands/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Constants/` — Centralizes identifiers used to select localized messages and validation errors.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/Logging/` — Defines typed log messages and reusable scopes for bot operations.
- `DC bot/Exceptions/Localization/` — Represents failures while loading or resolving translation resources.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
