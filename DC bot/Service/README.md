# Service

This folder contains the business logic layer of the application.

## Overview

Services implement core business logic separated from presentation (commands) and data access. Services delegate to
external dependencies and other services.

## Root Files

### BotService.cs

**Purpose:** Bot lifecycle management.

**Methods:**

- `StartAsync()` - Connect Discord client and run bot

**Usage:**

```csharp
var botService = new BotService(discordClient, logger);
await botService.StartAsync();
```

---

### LocalizationService.cs

**Purpose:** Multi-language support and guild language preferences.

**Implements:** `ILocalizationService`

**Methods:**

- `Get()` - Retrieve localized string by key
- `LoadLanguage()` - Load language for guild
- `SaveLanguage()` - Save guild's language preference

---

### ReactionHandler.cs

**Purpose:** Handle Discord message reactions for music control.

**Methods:**

- `RegisterHandler()` - Register reaction event handlers
- `SendReactionControlMessage()` - Send control panel with reactions

**Features:**

- Listens to message reactions
- Sends reaction control messages for music playback
- Integrates with `ILavaLinkService` events

---

## Subfolders

### Core/

Core orchestration services.

**Services:**

- `CommandHandlerService.cs` - Route messages to commands
- `CommandValidationService.cs` - Command validation logic
- `ValidationService.cs` - User, player, and connection validation

---

### Music/

Music playback and queue services.

**Main Services:**

- `LavaLinkService.cs` - Lavalink audio server orchestration
- `TrackSearchResolverService.cs` - URL/query resolution

**Subfolder:** `MusicServices/` - Granular music component services

---

### Presentation/

Response and communication services.

**Services:**

- `ResponseBuilder.cs` - Discord message responses

---

## Service Architecture

```
Commands
   ↓
ICommandHelper / IValidationService
   ↓
Core Services
   ↓
Music Services / Localization Services
   ↓
ILavaLinkService / IFileSystem
   ↓
Lavalink / File System / Discord
```

## Related Components

- **Interface/Service/** - Service contracts
- **Commands/** - Consume services
- **Wrapper/** - Discord abstraction

