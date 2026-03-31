# MusicServiceInterface

This folder contains granular music service interfaces.

## Overview

These interfaces split music domain responsibilities into focused contracts.

## Files

- `ICurrentTrackService.cs` - Current track management
- `IMusicQueueService.cs` - Queue operations
- `IPlaybackEventHandlerService.cs` - Playback event handling
- `IPlayerConnectionService.cs` - Player connection management
- `IRepeatService.cs` - Repeat mode logic
- `ITrackEndedHandlerService.cs` - Track end event handling
- `ITrackFormatterService.cs` - Track formatting for display
- `ITrackNotificationService.cs` - Track notification messages
- `ITrackPlaybackService.cs` - Track playback control

## Implementations

All implementations are in `Service/Music/MusicServices/` with matching filenames.

**Example:**

- `IMusicQueueService.cs` → `Service/Music/MusicServices/MusicQueueService.cs`
- `IRepeatService.cs` → `Service/Music/MusicServices/RepeatService.cs`

## Related Components

- **Service/Music/MusicServices/** - Implements these interfaces
- **Service/Music/LavaLinkService.cs** - Orchestrates these services

