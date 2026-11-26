# presentation

Guild-scoped audio-effect drafts, events, effects and ViewModel lifecycle.

## Overview

Preset saving uses `SaveAudioEffectPresetUseCase` and `AudioEffectRepository.saveAudioEffectPreset`, which POST a typed `name`/`commands`/`overwrite` body to `/guilds/{guildId}/playback/audio-effects/presets`. `ApplyAudioEffectsUseCase` continues to use `/guilds/{guildId}/playback/audio-effects`; saving never invokes that use case. The unapplied draft remains editable after saving.

`PlaybackMonitorObserver` owns playback observation and stops when the editing session closes. `CommandLock<AudioEffectsUiState>` serializes submissions, preset paging and play/pause. `AudioEffectsUiState` implements `CommandUiState` through the existing `busy` flag; session-version checks keep an old completion from releasing a newer session's lock.

Preset saving remains a `SavePreset` event handled by `AudioEffectsViewModel`: it sends a typed save-only request through `SaveAudioEffectPresetUseCase`, waits for terminal completion, and refreshes preset metadata without discarding the unapplied draft. There is no separate audio-effect command-handler class.

`AudioEffectsProfileMapper` recognizes semantic profile identity from settings without Compose dependencies. Display formatting and localized resource mapping remain in the UI module.

The module uses `commonMain` for shared Android/iOS functionality and depends on `core:domain`, `core:model`, `core:presentation` and `core:common`; the module is independent of CurrentTrack and Compose rendering. Android host compilation does not validate iOS; iOS framework builds require macOS/Xcode.

## Subfolders

### src/

Platform-independent implementation and configured source sets.

**Files:**

- `AudioEffectsViewModel.kt`
- `AudioEffectsEvent.kt`
- `AudioEffectsEffect.kt`
- `state/AudioEffectsUiState.kt`
- `model/AudioEffectControl.kt`
- `mapper/AudioEffectsProfileMapper.kt`
- `di/AudioEffectModule.kt`

**Purpose:** Owns the presentation responsibilities of the audio-effect feature. Test scaffolds remain unimplemented and must not be counted as passing behavioral tests.

---

## Module Configuration

### build.gradle.kts

**Purpose:** Configures the Android library and `iosArm64` / `iosSimulatorArm64` frameworks using the shared version catalog. The new module is included by the application through `shared`.

**Validation Checks:**

- `./gradlew.bat :androidApp:assembleDebug` verifies Android compilation and packaging of both modules.

## Related Components

- [audioeffect](../README.md) - Feature behavior, source ownership and navigation.
- [MobileApp](../../../README.md) - Application structure and build commands.
