# MusicServiceInterface

This folder contains granular music service interfaces.

## Overview

These interfaces split music domain responsibilities into focused contracts.

## Files

- `ICurrentTrackService.cs` - Current track management
- `ILavalinkNodeConnectionService.cs` - Lavalink node lifecycle connection
- `IMusicQueueService.cs` - Queue operations, including repeat-list snapshot rehydration
- `IPlaybackControlService.cs` - Pause, resume, skip, and leave control operations
- `IPlaybackEventHandlerService.cs` - Playback event handling
- `IPlaybackRequestService.cs` - Play request orchestration for URL/query input
- `IPlayerConnectionService.cs` - Player connection management
- `IRepeatService.cs` - Repeat mode flags and repeat-list snapshot writes
- `ITrackEndedHandlerService.cs` - Track end event handling
- `ITrackFormatterService.cs` - Track formatting for display
- `ITrackNotificationService.cs` - Track notification messages
- `ITrackPlaybackService.cs` - Track playback control

## Implementations

All implementations are in `Service/Music/MusicServices/` with matching filenames.

**Example:**

- `IMusicQueueService.cs` → `Service/Music/MusicServices/MusicQueueService.cs`
- `ILavalinkNodeConnectionService.cs` → `Service/Music/MusicServices/LavalinkNodeConnectionService.cs`
- `IPlaybackControlService.cs` → `Service/Music/MusicServices/PlaybackControlService.cs`
- `IPlaybackRequestService.cs` → `Service/Music/MusicServices/PlaybackRequestService.cs`
- `IRepeatService.cs` → `Service/Music/MusicServices/RepeatService.cs`

## Related Components

- **Service/Music/MusicServices/** - Implements these interfaces
- **Service/Music/LavaLinkService.cs** - Orchestrates these services

