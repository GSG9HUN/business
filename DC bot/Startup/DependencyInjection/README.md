# DependencyInjection

Registers command, core, Discord, Lavalink, logging, and music dependencies at the composition root.

## Overview

- Personal favorites register only `FavoriteCommand` and `FavoriteSlashCommand`; the command modules reuse the existing favorite service.

- Text registration uses `QueueCommand` and `ImportPlaylistCommand`; slash adapters dispatch `queue` and `importPlaylist` through the shared executor.

- Composes application services, command adapters and external clients.
- Defines dependency lifetimes, configuration validation and SDK event wiring.
- Connects bot runtime modules to shared persistence registrations.

## Classes

### CommandServiceCollectionExtensions

**File:** `DC bot/Startup/DependencyInjection/CommandServiceCollectionExtensions.cs`

**Purpose:** Registers the command runtime dependencies at the composition root.

Registers text `ICommand` implementations as singletons, slash adapters as transients, and slash types in the DSharpPlus inventory. Entry points share validation and localized presentation.

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

Registers `IHelpResponseBuilder` / `HelpResponseBuilder` and `DiscordPaginationService` as singletons alongside the response builder and localization service. The help builder owns metadata/layout; pagination retains the existing interaction event and message lifecycle.

Supplies the shared `IResponseBuilder` to command routing, reaction presentation and durable-command notifications, including the explicit reaction-message factory.

Registers routing, durable commands, localization and response presentation. `Options.Create(botSettings)` supplies the same prefix to usage rendering and message dispatch.

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

Component events reach both pagination and search-picker handlers; each accepts only its own component ID prefix.

Wires gateway, message, reaction, voice and component-interaction callbacks to injected handlers, including music-search selection.

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

Configures Lavalink clients and the authenticated `MusicDiscovery` HTTP client for provider-backed discovery.

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

Registers playback, filters, discovery, queues and guild controls. Presets use `IFilterPresetRepository`; queue/playlist limits use active premium. `MUSIC_JOIN_DEADLINE` and `MUSIC_LEAVE_DEADLINE` must be positive supported `TimeSpan` values. `BotApplication.RunAsync` explicitly starts the shared persistent-voice maintenance worker.

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

## Related Components

- `DC bot/Startup/README.md` — Parent feature overview and neighboring components.
- `DC bot/README.md` — Project setup and architecture.
- `DC bot/Configuration/` — Defines typed options for bot identity, Lavalink connectivity, search resolution, and saved-playlist limits.
- `DC bot/Interface/` — Defines command and adapter boundaries used inside the bot executable.
- `DC bot/IO/` — Implements the injected file-system boundary for translation and guild preference data.
- `DC bot/Service/` — Groups process lifecycle, localization, command orchestration, music, reactions, and slash execution.
- `DC bot/Startup/DependencyInjection/` — Registers the runtime implementations used by these components.
