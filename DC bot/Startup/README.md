# Startup

Loads runtime settings, composes dependencies, applies migrations, enables handlers, and observes shutdown.

## Overview

- BotApplication validates lifecycle options before migrations, then runs the Discord service, mobile command worker, pending-start recovery, and persistent voice recovery together under linked cancellation.
- The bot uses ServiceProvider directly rather than an ASP.NET Host. Explicit options resolution makes validation happen before external startup, and worker faults must not be hidden as successful cancellation.

## Subfolders

### DependencyInjection/

Registers command, core, Discord, Lavalink, logging, and music dependencies at the composition root.

**Files:**

- `CommandServiceCollectionExtensions.cs`
- `CoreServiceCollectionExtensions.cs`
- `DiscordServiceCollectionExtensions.cs`
- `LavalinkServiceCollectionExtensions.cs`
- `LoggingServiceCollectionExtensions.cs`
- `MusicServiceCollectionExtensions.cs`

**Purpose:** BotServiceProviderFactory invokes these modules and AddPersistenceServices. Discord builder callbacks route gateway, message, reaction, and voice events to injected handlers.

---

## Classes

### BotApplication

**File:** `DC bot/Startup/BotApplication.cs`

**Purpose:** Loads configuration, applies migrations, activates handlers, and starts the runtime.

**Methods:**

- `RunAsync` — Loads settings, composes services, applies database migrations, activates handlers, and runs the Discord client.

```csharp
public static async Task RunAsync(
    TextWriter? output = null,
    bool isTestEnvironment = false,
    CancellationToken cancellationToken = default);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
await BotApplication.RunAsync(output, isTestEnvironment, cancellationToken);
```

---

### BotConfigurationLoader

**File:** `DC bot/Startup/BotConfigurationLoader.cs`

**Purpose:** Loads environment-based Discord, Lavalink, and PostgreSQL startup settings.

**Methods:**

- `BuildPostgresConnectionString` — Builds the PostgreSQL connection string from startup environment values and defaults.

```csharp
internal static string BuildPostgresConnectionString();
```

- `LoadFromEnvironment` — Loads runtime settings and reports missing required startup configuration.

```csharp
public static BotRuntimeSettings? LoadFromEnvironment(TextWriter output);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = BotConfigurationLoader.BuildPostgresConnectionString();
```

---

### BotHandlerRegistrar

**File:** `DC bot/Startup/BotHandlerRegistrar.cs`

**Purpose:** Activates and registers runtime handlers after dependency injection is composed.

**Methods:**

- `RegisterHandlers` — Registers runtime event handlers while preventing duplicate subscriptions.

```csharp
public static void RegisterHandlers(IServiceProvider services);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
BotHandlerRegistrar.RegisterHandlers(services);
```

---

## Factory Classes

### BotServiceProviderFactory

**File:** `DC bot/Startup/BotServiceProviderFactory.cs`

**Purpose:** Builds the provider from runtime settings and domain registration extensions.

**Methods:**

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public static ServiceProvider Create(
    BotSettings botSettings,
    LavalinkSettings lavalinkSettings,
    string? postgresConnectionString = null);
```

- `Create` — Constructs the adapter or runtime object from supplied context and dependencies.

```csharp
public static ServiceProvider Create(BotRuntimeSettings settings);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = BotServiceProviderFactory.Create(botSettings, lavalinkSettings, postgresConnectionString);
```

---

## Models and Results

### BotRuntimeSettings

**File:** `DC bot/Startup/BotRuntimeSettings.cs`

**Purpose:** Groups bot, Lavalink, and database settings needed by application startup.

**Record declaration:**

```csharp
internal sealed record BotRuntimeSettings(
    BotSettings BotSettings,
    LavalinkSettings LavalinkSettings,
    string PostgresConnectionString);
```

---

## Related Components

- `DC bot/README.md` — Parent feature overview and neighboring components.
- `DC bot/Startup/DependencyInjection/README.md` — Detailed component contracts and examples for this subfolder.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Service/` — Groups process lifecycle, localization, command orchestration, music, reactions, and slash execution.
- `DC bot/Service/Core/` — Handles text command routing, registry snapshots, argument parsing, and user/player readiness validation.
- `DC bot/Service/ReactionHandler/` — Connects now-playing messages and Discord reactions to music controls.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
