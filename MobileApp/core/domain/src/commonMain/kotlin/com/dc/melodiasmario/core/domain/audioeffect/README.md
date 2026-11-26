# audioeffect

Authorized audio-effect repository contracts and Resource-returning use cases.

## Overview

The explicit `saveAudioEffectPreset` operation persists the draft through POST `/guilds/{guildId}/playback/audio-effects/presets`. It does not use `applyAudioEffects` and never applies live settings. The save request carries the preset name, setting edits and overwrite consent; the existing command coordinator still waits for terminal completion.

This package is in `commonMain` and depends on `core:model` and `core:common`; concrete data sources and UI are outside its dependency direction.

`GetAudioEffectsUseCase` reads confirmed settings and preset pages. `ApplyAudioEffectsUseCase` submits effect operations, while `SaveAudioEffectPresetUseCase` submits preset saves. All three emit loading, success or error while preserving coroutine cancellation. Koin discovers the use cases through the existing domain component scan.

## Subfolders

### usecase/

User-facing audio-effect operations.

**Files:**

- `usecase/ApplyAudioEffectsUseCase.kt`
- `usecase/GetAudioEffectsUseCase.kt`
- `usecase/SaveAudioEffectPresetUseCase.kt`

**Purpose:** Coordinates repository calls and recoverable errors.

---

## Components

### AudioEffectRepository

**Purpose:** Defines effect and preset data access.

---

## Related Components

- `core:network` - HTTP transport and remote data sources.
- `core:domain` - Repository contracts and use cases.
- `core:data` - Authorized repository implementations.
- `core:model` - Shared platform-independent values.
- `feature:audioeffect:presentation` - Draft editing and command completion.
