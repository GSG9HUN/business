# API

This ASP.NET Core minimal API exposes mobile authentication, guild and profile reads, saved playlists, music search, playback control, and SignalR updates. It uses the shared PostgreSQL persistence layer rather than referencing the running Discord bot executable.

## Overview

- Request, response, and options members document accepted input, OAuth values, search capabilities, and machine-readable error outcomes in their source XML comments.
- Expose authenticated mobile HTTP endpoints and SignalR subscriptions.
- Coordinate OAuth, profile and guild reads, music search, and durable bot commands.
- Map shared repository values into HTTP and realtime payloads.

## Runtime Flow

```text
Mobile HTTP request
  -> authentication + endpoint filters
  -> handler + stored guild-access checks
  -> repository query / metadata update / durable bot command

Durable bot command -> bot worker -> command result and playback state
  -> PostgreSQL notification -> realtime listener
  -> snapshot + authorized SignalR groups -> mobile client
```

HTTP acceptance of a bot command means the request was stored, not that audio has already changed. Clients can poll its status and subscribe to realtime progress. Profile, playlist metadata, and explicit repeat-mode operations also have direct persistence paths; not every write is a worker command.

## Run Locally

From the repository root, with the .NET 9 SDK and the intended database available:

```bash
dotnet restore "DC bot.sln"
dotnet build "API/API.csproj"
dotnet run --project "API/API.csproj"
```

`Program.cs` searches the current-directory and application-base ancestor chains for an optional `.env`, loading the first match without overwriting existing process environment values. It maps required environment values into the typed authentication configuration.

| Required environment values | Purpose |
|---|---|
| `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DB`, `POSTGRES_USER`, `POSTGRES_PASSWORD` | Shared PostgreSQL connection. |
| `APP_AUTH_SIGNING_KEY`, `APP_AUTH_ISSUER`, `APP_AUTH_AUDIENCE` | Application JWT signing and validation. |
| `DISCORD_OAUTH_CLIENT_ID`, `DISCORD_OAUTH_CLIENT_SECRET` | Discord OAuth application credentials. |
| `DISCORD_OAUTH_REDIRECT_URI` | Browser callback registered for Discord OAuth. |
| `DISCORD_OAUTH_ANDROID_REDIRECT_URI` | Mobile deep-link destination for login-ticket return. |

`AppAuthOptions` and `DiscordOAuthOptions` are bound and validated at startup. Music search uses the `MusicSearch` configuration section; `LAVALINK_HOSTNAME`, `LAVALINK_PORT`, `LAVALINK_SECURED`, and `LAVALINK_PASSWORD` can supply its node connection settings. Provider rollout switches determine which search and enqueue capabilities are exposed.

The API registers persistence but does not apply migrations in its current startup path. Use bot startup or the documented [migration workflow](../DC%20bot.Persistence/Migrations/README.md) before relying on a new schema.

## Application Entry Point

### Program.cs

**Purpose:** Loads configuration, registers services, builds authentication and middleware, and maps the HTTP and SignalR endpoints.

**Configuration helper:**

```csharp
static string GetRequiredEnvironmentVariable(string name);
```

**Validation Checks:** Missing required environment values fail before the application is built. Typed authentication and music-search options are validated during startup.

---

## Authentication and Authorization

The login flow creates cached OAuth state, redirects the browser to Discord, synchronizes the returned user and guild metadata, and redirects to the mobile URI with a short-lived ticket. The mobile exchange creates a persisted session and returns a 15-minute access token plus an opaque refresh token. Only the refresh-token hash is stored; refresh uses conditional rotation of that hash.

OAuth state and login tickets use process-local memory cache. A deployment with multiple API instances needs shared state or consistent routing for that login flow. The bearer token identifies the mobile user; handlers still check persisted guild access before exposing guild data or accepting controls.

## HTTP and Realtime Boundaries

Endpoint groups are mounted under `/api`; the authenticated SignalR hub is `/hubs/mobile`. OpenAPI is mapped only in the development environment. [Endpoints](Endpoints/README.md) links to the route registrations, which are the source of truth for paths, filters, and handler bindings.

Handlers coordinate requests, [Mapping](Mapping/README.md) converts results and records, and [Responses](Responses/README.md) defines client payloads. Shared [playback snapshots](Snapshots/Playback/README.md) keep REST and realtime read models aligned. Snapshots describe persisted state; they do not independently certify a live voice connection.

The PostgreSQL realtime listener treats notifications as invalidation signals and reloads current state. Guild publishers resolve authorized subscribers before sending. Request cancellation reaches repository and HTTP calls; hub subscription operations use the connection token, while disconnect cleanup removes memberships independently of that aborted token.

## Subfolders

### Endpoints/

Registers minimal API routes, handler bindings, authorization, and endpoint filters.

**Files:**

- `AuthEndpoints.cs`
- `CommandEndpoints.cs`
- `GuildEndpoints.cs`
- `MusicSearchEndpoints.cs`
- `PlaybackEndpoints.cs`
- `PlayerEndpoints.cs`
- `PlaylistEndpoints.cs`
- `ProfileEndpoints.cs`
- `QueueEndpoints.cs`
- `StatusEndpoints.cs`

**Purpose:** Program.cs mounts these groups under /api. Route registration selects the handler; handlers own repository calls and response decisions.

---

### Errors/

Defines error categories understood by HTTP result mapping.

**Files:**

- `ApiErrorCode.cs`

**Purpose:** Handlers create expected failures with ApiErrorCode; DomainToHttpMapper selects their HTTP status and body.

---

### Handlers/

Coordinates HTTP input, authenticated user context, persistence contracts, and API response mapping.

**Files:**

- `ApiUserContext.cs`

**Purpose:** ApiUserContext resolves the JWT subject and checks stored guild access before feature handlers read or change guild data.

---

### Mapping/

Converts repository records and service outcomes into HTTP response contracts.

**Files:**

- `BotControlCommandHttpMapper.cs`
- `DomainToHttpMapper.cs`
- `GuildResponseMapper.cs`
- `QueueResponseMapper.cs`
- `TrackResponseMapper.cs`

**Purpose:** TrackResponseMapper decodes stored Lavalink identities; queue and playlist mapping omits undecodable tracks. DomainToHttpMapper handles expected API failures.

---

### Properties/

Contains launch profiles used by local ASP.NET Core development tools.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** launchSettings.json supplies local process URLs and launch options. Container binding is configured separately by Docker Compose.

---

### Realtime/

Hosts authenticated SignalR subscriptions and forwards persisted state changes to mobile clients.

**Files:**

- `MobileUpdateGroups.cs`
- `MobileUpdatesHub.cs`

**Purpose:** Repositories publish PostgreSQL notifications; the listener loads snapshots and publishers send events to authorized per-user or per-user-guild groups.

---

### Repositories/

Reserves a location for genuinely API-specific storage adapters; no repository implementation currently lives here.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** The running API resolves repository contracts from DC bot.Contracts through AddPersistenceServices in DC bot.Persistence.

---

### Requests/

Groups client input by feature before handler validation and orchestration.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** Minimal API binding creates these values; filters and handlers interpret them and build repository inputs or bot-command payloads.

---

### Responses/

Groups stable client-facing output shapes for auth, guilds, playback, queues, profiles, playlists, and music search.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** Handlers, mappers, and snapshot providers construct these values from shared repository records and service results.

---

### Results/

Represents either a successful value or an expected error before conversion to HTTP.

**Files:**

- `ApiResult.cs`

**Purpose:** ApiResult<T> is consumed by DomainToHttpMapper. OAuth redirects and accepted bot commands can return IResult directly when their HTTP semantics differ.

---

### Services/

Groups API-owned authentication and music-search behavior that is larger than a route declaration.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** Auth services manage OAuth and token creation; music-search services validate provider queries, inspect node capabilities, and map results.

---

### Snapshots/

Groups snapshot assembly used by both HTTP reads and realtime events.

**Files:**

- `README.md` — Overview and navigation to feature-specific source files.

**Purpose:** The playback implementation combines repositories and mapping rather than reading a live Discord or Lavalink player.

---

### Validation/

Checks route identifiers, command aliases, and provider-aware enqueue input before handlers run.

**Files:**

- `CommandNameValidationFilter.cs`
- `GuildIdValidationFilter.cs`
- `MusicEnqueueValidationFilter.cs`
- `RequestValidation.cs`

**Purpose:** GuildIdValidationFilter and CommandNameValidationFilter store normalized route values in HttpContext.Items. MusicEnqueueValidationFilter checks query shape and deployed provider enqueue permission.

---

## Deployment and Verification

The root Compose stack builds the `discord-api` service from `API/Dockerfile`, supplies the root `.env`, and configures the container to listen on port 8080. Host mapping and network addresses are defined in [docker-compose.yaml](../docker-compose.yaml), separately from local launch profiles.

Use the [test project](../DC%20bot%20tests/README.md) for focused auth and cancellation tests. Live authentication redirects, voice playback, and mobile subscriptions require their configured external environment. Shared protocol changes must stay compatible with [Contracts](../DC%20bot.Contracts/README.md), [Persistence](../DC%20bot.Persistence/README.md), and the [bot worker](../DC%20bot/Service/BotControl/README.md).

## Related Components

- `DC bot/` — Discord commands, live playback lifecycle, and the mobile command worker.
- `API/` — HTTP request handling, authentication, response mapping, and SignalR publication.
- `DC bot.Contracts/` — Shared storage interfaces, immutable models, command payloads, and protocol names.
- `DC bot.Persistence/` — PostgreSQL repository implementations, migrations, and database notifications.
- `DC bot tests/` — Behavior tests and explicitly skipped planned cases.
- `AGENTS.md` — Implementation and detailed README conventions.
