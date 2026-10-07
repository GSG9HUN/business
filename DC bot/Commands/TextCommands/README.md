# Text Commands

This folder contains prefix-based Discord text commands grouped by feature.

## Subfolders

### Music/

Playback commands such as join, play, pause, resume, skip, previous, and leave.

### Queue/

Queue commands such as view queue, clear, shuffle, repeat track, repeat list, and queue item movement/removal.

### Playlist/

Saved playlist commands for create, save, load, view, list, rename, delete, and track mutation.

### Utility/

General utility commands such as help, ping, language, and tag.

## Flow

```text
Discord message
  -> CommandHandlerService
  -> ICommandRegistry
  -> ICommand.ExecuteAsync
  -> validation + domain service
  -> localized response
```

## Notes

- Text commands own Discord message parsing and user-facing responses.
- Business behavior belongs in services, not command classes.
- Slash commands reuse this pipeline through `ISlashCommandExecutor`.
