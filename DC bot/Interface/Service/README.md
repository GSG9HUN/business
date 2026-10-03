# Service Interfaces

This folder contains service abstraction contracts grouped by domain.

## Subfolders

### IO/

File system service interfaces.

**File:** `IFileSystem.cs`

---

### Localization/

Localization service interfaces.

**File:** `ILocalizationService.cs`

---

### Music/

Music and playback service interfaces.

**Files:**

- `ILavaLinkService.cs`
- `ITrackSearchResolverService.cs`
- `Music/` - Granular music service interfaces
- `PlaylistServiceInterface/` - Saved playlist service contract and result DTOs
- `ProgressiveTimerInterface/` - Now-playing timer service interface

---

### Persistence Contracts

Persistence repository contracts used by services moved to `../../../DC bot.Contracts/Interface/Service/Persistence/`.

**Files:**

- `IGuildDataRepository.cs`
- `IPlaybackStateRepository.cs`
- `IPlaylistRepository.cs`
- `IPlaylistTrackRepository.cs`
- `IQueueRepository.cs`
- `IRepeatListRepository.cs`
- `IBotControlCommandsRepository.cs`
- `Models/` - contract record models

---

### BotControl/

Bot-control execution contracts used by the bot worker for API-originated commands.

**Files:**

- `IBotControlWorker.cs`
- `IBotControlCommandDispatcher.cs`
- `IBotControlContextResolver.cs`
- `IBotControlDiscordResponseSink.cs`
- `IBotControlResultFactory.cs`
- `Models/` - command execution context and result records

---

### Presentation/

Response and presentation interfaces.

**File:** `IResponseBuilder.cs`

---

### SlashCommands/

Slash command adapter contracts.

**Files:**

- `ISlashCommandExecutor.cs`
- `ISlashInteractionContext.cs`
- `ISlashInteractionContextFactory.cs`
- `SlashCommandExecutionRequest.cs`

---

## Related Components

- **Service/** - Implements these interfaces
- **DC bot.Persistence/** - Implements persistence contracts
- **Commands/** - Use service interfaces for business logic

