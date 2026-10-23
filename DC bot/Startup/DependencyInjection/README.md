# DependencyInjection

Registers command, core, Discord, Lavalink, logging, and music dependencies at the composition root.

## Overview

- BotServiceProviderFactory invokes these modules and AddPersistenceServices. Discord builder callbacks route gateway, message, reaction, and voice events to injected handlers.
- Keep concrete construction and factory wiring here. MUSIC_JOIN_DEADLINE and MUSIC_LEAVE_DEADLINE must be positive supported TimeSpan values; they are validated before runtime work.

## Classes

### CommandServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/CommandServiceCollectionExtensions.cs`

**Purpose:** Registers the command runtime dependencies at the composition root.

**Methods:**

- `AddCommandServices` — Registers the command services dependencies with the shared service collection.

```csharp
public static IServiceCollection AddCommandServices(this IServiceCollection services);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddCommandServices();
```

---

### CoreServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/CoreServiceCollectionExtensions.cs`

**Purpose:** Registers the core runtime dependencies at the composition root.

**Methods:**

- `AddCoreBotServices` — Registers the core bot services dependencies with the shared service collection.

```csharp
public static IServiceCollection AddCoreBotServices(this IServiceCollection services, BotSettings botSettings);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddCoreBotServices(botSettings);
```

---

### DiscordServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/DiscordServiceCollectionExtensions.cs`

**Purpose:** Registers the discord runtime dependencies at the composition root.

**Methods:**

- `AddDiscordRuntime` — Registers the discord runtime dependencies with the shared service collection.

```csharp
public static IServiceCollection AddDiscordRuntime(this IServiceCollection services, string discordToken);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddDiscordRuntime(discordToken);
```

---

### LavalinkServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/LavalinkServiceCollectionExtensions.cs`

**Purpose:** Registers the lavalink runtime dependencies at the composition root.

**Methods:**

- `AddLavalinkRuntime` — Registers the lavalink runtime dependencies with the shared service collection.

```csharp
public static IServiceCollection AddLavalinkRuntime(this IServiceCollection services, LavalinkSettings lavalinkSettings);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddLavalinkRuntime(lavalinkSettings);
```

---

### LoggingServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/LoggingServiceCollectionExtensions.cs`

**Purpose:** Registers the logging runtime dependencies at the composition root.

**Methods:**

- `AddBotLogging` — Registers the bot logging dependencies with the shared service collection.

```csharp
public static IServiceCollection AddBotLogging(this IServiceCollection services);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddBotLogging();
```

---

### MusicServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs`

**Purpose:** Registers the music runtime dependencies at the composition root.

**Methods:**

- `AddMusicServices` — Registers the music services dependencies with the shared service collection.

```csharp
public static IServiceCollection AddMusicServices(this IServiceCollection services);
```

**Usage:**

The caller supplies the component and argument values used in this example.

```csharp
var result = services.AddMusicServices();
```

---

## Twenty-Four-Seven Registration

**Purpose:** Connect both Discord command entry points to the same settings workflow.

- `AddTextCommands` registers `TwentyFourSevenCommand` as an `ICommand` singleton.
- `AddSlashCommandServices` registers `TwentyFourSevenSlashCommand` as transient; `AddSlashCommandProcessor` adds its command type to the extension.
- `AddMusicServices` registers `TwentyFourSevenService`, `DisconnectedPlayerCleanupService`, `PersistentVoiceGate`, `PersistentVoiceLeaveService`, and `PersistentVoiceRecoveryService` as singletons. `BotApplication.RunAsync` explicitly starts the persistent voice worker alongside the existing workers; this runtime uses a `ServiceProvider`, so registration alone does not start a hosted service.
- `IGuildDataRepository` remains provided by the persistence registration.

---

## Related Components

- `DC bot/Startup/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/IO/` — Implements the injected file-system boundary for translation and guild preference data.
- `DC bot/Service/` — Groups process lifecycle, localization, command orchestration, music, reactions, and slash execution.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
