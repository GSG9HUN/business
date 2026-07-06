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
- `MusicServiceInterface/` - Granular music service interfaces
- `ProgressiveTimerInterface/` - Now-playing timer service interface

---

### Persistence/

Persistence repository contracts used by services.

**Files:**

- `IGuildDataRepository.cs`
- `IPlaybackStateRepository.cs`
- `IQueueRepository.cs`
- `IRepeatListRepository.cs`
- `Models/` - contract record models

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
- **Persistence/** - Implements persistence contracts
- **Commands/** - Use service interfaces for business logic

