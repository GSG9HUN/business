# Program.cs and Startup

This document explains the bot process entry point, dependency composition, migration invocation, and coordinated runtime lifetime.

## Overview

- Load an optional `.env` from the current working directory without overwriting process environment values.
- Build bot, Lavalink, and PostgreSQL settings and register the runtime service graph.
- Validate playback deadlines before migrations or Discord startup.
- Run the Discord lifecycle, mobile command worker, and playback recovery under linked cancellation.

## Entry Point

### Program

**File:** `DC bot/Program.cs`

**Purpose:** Owns the process shutdown token and delegates application startup.

**Method:**

```csharp
private static async Task Main();
```

**Behavior:**

- `Console.CancelKeyPress` cancels the shutdown token.
- The optional environment file is read from the current working directory only.
- `BotApplication.RunAsync` receives the token and owns the application flow.

**Usage:**

Build from the repository root, then start from the bot project directory so the `localization/` and `guildFiles/` paths resolve:

```bash
dotnet build "DC bot.sln"
cd "DC bot"
dotnet run --project "DC bot.csproj"
```

Provide the required process environment values or a local `.env` in that working directory. Unlike the API loader, the bot entry point does not search parent directories for `.env`. Docker Compose explicitly passes the repository-root file to the container.

---

## Startup Classes

### BotApplication

**File:** `DC bot/Startup/BotApplication.cs`

**Purpose:** Coordinates configuration, service-provider lifetime, startup checks, and runtime workers.

**Method:**

```csharp
public static Task RunAsync(
    TextWriter? output = null,
    bool isTestEnvironment = false,
    CancellationToken cancellationToken = default);
```

**Startup sequence:**

1. Check cancellation and load runtime settings.
2. Create the service provider and explicitly resolve validated `PlaybackLifecycleOptions`.
3. Apply pending EF migrations with the application token.
4. Enable command and reaction handlers.
5. Start the bot service, mobile command worker, and playback recovery loop.
6. Cancel sibling tasks when one finishes and observe their results before disposing the provider.

**Validation Checks:**

- Missing `DISCORD_TOKEN` or `LAVALINK_HOSTNAME` is reported by the configuration loader before the runtime starts.
- `MUSIC_JOIN_DEADLINE` and `MUSIC_LEAVE_DEADLINE` are required, positive supported `TimeSpan` values; invalid options fail before migration and Discord connection.
- Caller-requested shutdown is distinguished from a fault in one of the workers.

The explicit test-runtime branch starts the bot service in test mode and does not run live node recovery.

---

### BotConfigurationLoader

**File:** `DC bot/Startup/BotConfigurationLoader.cs`

**Purpose:** Converts environment values into `BotRuntimeSettings` and builds the runtime PostgreSQL connection string.

**Method:**

```csharp
public static BotRuntimeSettings? LoadFromEnvironment(TextWriter output);
```

**Configuration:**

The complete variable/default table is in `DC bot/README.md`. Provider credentials consumed by Lavalink are separate from the bot's configuration loader. The C# loader builds its database string from `POSTGRES_*`; it does not read `POSTGRES_CONNECTION_STRING`.

---

### BotServiceProviderFactory

**File:** `DC bot/Startup/BotServiceProviderFactory.cs`

**Purpose:** Composes logging, core services, Discord callbacks, Lavalink, shared persistence, commands, and music services.

**Method:**

```csharp
public static ServiceProvider Create(BotRuntimeSettings settings);
```

`DC_bot.DependencyInjection.PersistenceServiceCollectionExtensions.AddPersistenceServices` registers database contexts and repositories from the separate persistence project. Music helpers, factories, and player/session services are provided by DI rather than fallback construction inside use cases.

---

### BotHandlerRegistrar

**File:** `DC bot/Startup/BotHandlerRegistrar.cs`

**Purpose:** Enables the command and reaction handlers after composition and migration.

Discord event callbacks are configured by `DiscordServiceCollectionExtensions.AddDiscordRuntime`. Registration and handler enablement are separate steps, so a gateway callback cannot be assumed to mean that command processing is already active.

## Related Components

- `DC bot/README.md` — Runtime setup, configuration, and playback architecture.
- `DC bot/Startup/README.md` — Startup components and their current signatures.
- `DC bot/Startup/DependencyInjection/README.md` — Registration modules and examples.
- `DC bot.Persistence/Db/README.md` — Context factory and migration runner.
- `DC bot/Service/BotControl/README.md` — Durable API command execution.
- `DC bot/Service/Music/Lifecycle/README.md` — Session ownership, deadlines, cleanup, and recovery.
