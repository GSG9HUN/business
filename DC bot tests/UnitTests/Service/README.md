# Service Unit Tests

This folder contains isolated service-level unit tests.

## Scope

Service tests verify orchestration and edge-case behavior without real Discord, Lavalink, database, or network dependencies. External services are mocked, while focused helpers and in-memory fakes are used where they make behavior easier to reason about.

## Subfolders

### Music/

Music service tests cover queue behavior, player connection handling, playback control, track playback, repeat state, current-track persistence boundaries, playlist service behavior, and progressive timer lifecycle.

### Core/

Core service tests cover command lookup, command validation, and service-level command infrastructure.

### Localization/

Localization tests cover default translations, guild language handling, formatting fallback, directory handling, and error paths.

### ReactionHandler/

Reaction handler tests cover context creation, action dispatch, control message publishing, registration, dispatch, and exception logging.

## Run

```bash
dotnet test "DC bot tests/DC bot tests.csproj" --filter "Category=Unit"
```
