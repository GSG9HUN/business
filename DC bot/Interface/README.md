# Interface

This folder contains all interface definitions (contracts) for the application.

## Overview

Interfaces enable:

- **Loose coupling** - Components depend on abstractions
- **Testability** - Easy to mock in unit tests
- **Flexibility** - Swap implementations without changing consumers

## Root Interfaces

### ICommand.cs

Base contract for all text commands:

```csharp
public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task ExecuteAsync(IDiscordMessage message);
}
```

**Implementations:** All classes in `Commands/` (except SlashCommands)

---

### ILavaLinkTrack.cs

Contract for music track abstraction:

```csharp
public interface ILavaLinkTrack
{
    string Title { get; }
    string Author { get; }
    TimeSpan Duration { get; }
    TimeSpan? StartPosition { get; }
    Uri? ArtworkUri { get; }
    LavalinkTrack ToLavalinkTrack();
    string ToString();
}
```

**Implementation:** `Wrapper/LavalinkTrackWrapper.cs`

---

## Subfolders

### Core/

Core application interfaces.

**Files:**

- `ICommandHelper.cs` - Command validation helpers
- `IValidationService.cs` - Player/connection validation
- `IUserValidationService.cs` - User validation

---

### Discord/

Discord abstraction interfaces.

**Files:**

- `IDiscordMessage.cs`
- `IDiscordChannel.cs`
- `IDiscordUser.cs`
- `IDiscordMember.cs`
- `IDiscordGuild.cs`
- `IDiscordVoiceState.cs`

**Purpose:** Decouple from DSharpPlus library.

**Implementations:** `Wrapper/*.cs`

---

### Service/

Service layer interfaces grouped by domain.

**Subfolders:**

- `IO/` - File system interfaces
- `Localization/` - Localization service
- `Music/` - Music and playback services
- `Presentation/` - Response building

---

## Related Components

- **Commands/** - Implement `ICommand`
- **Service/** - Implement service interfaces
- **Wrapper/** - Implement Discord interfaces

