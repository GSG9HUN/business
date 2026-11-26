# audioeffect

Independent Kotlin Multiplatform audio-effect presentation and Compose UI. The feature is split into `:feature:audioeffect:presentation` and `:feature:audioeffect:ui`.

## Overview

Preset saving uses `SaveAudioEffectPresetUseCase` and `AudioEffectRepository.saveAudioEffectPreset`, which POST a typed `name`/`commands`/`overwrite` body to `/guilds/{guildId}/playback/audio-effects/presets`. `ApplyAudioEffectsUseCase` continues to use `/guilds/{guildId}/playback/audio-effects`; saving never invokes that use case. The unapplied draft remains editable after saving.

The audio-effect ViewModel observes shared playback through `PlaybackMonitorObserver` and uses `CommandLock` for foreground operations. Its `busy` flag implements `CommandUiState`, so command completion, failure and session disposal release the same lock without adding another busy flag.

Custom presets can save the edited draft without an earlier Apply and without changing live audio. Mobile sends the name, overwrite choice and draft setting edits to the dedicated preset endpoint; the API constructs the internal save operation, and the bot composes a definition from the authoritative snapshot under the existing save reservation, rechecks management/voice permissions, and persists it without committing filters to Lavalink. The dirty draft remains available for a later Apply. The custom-preset page uses a server callout and compact preset rows and ends with the create button and muted volume/24-7 exclusion note.

The EQ scrollbar shares the band row's measured scroll state and supports drag, track taps, accessible progress changes and right-to-left layouts. Hungarian/default and English resources contain matching keys and placeholder contracts; effect/profile names such as Nightcore and Equalizer intentionally remain identical.

Advanced sound includes a guild/voice-room top-bar subtitle separated by a Unicode middle dot, a cyan overline with 24sp headline and muted description, underline tabs, a cyan EQ reset at top right, the 15-band caption and visible horizontal-scroll indicator/hint. Filter rows open local-draft editing dialogs instead of switches. Only Apply advanced settings and Save as custom preset appear in the advanced footer. Keeping or cancelling a filter popup never sends a bot command; Apply commits the draft.

The audio-effect Screen has no Scaffold; the shared navigation layer owns page chrome and insets. Its Route collects effects sequentially and delegates localized feedback to `AudioEffectsEffectMessages` and the application toast host. Dialog-only form content stays in `AudioEffectsDialogs.kt`, related slider composables share `AudioSoundSlider.kt`, and substantial editor sections remain separate. Each visual component has previews beside its implementation, without a live ViewModel or network.

The sound editor includes live mini-player header, large icon profile tiles with a selected check mark, green confirmed-profile badge, right-aligned multiplier values, full-width icon actions, and the actual confirmed output-volume note. The basic page links to advanced sound; advanced back returns to basic settings, and basic back leaves without issuing a bot command. Mini-player play/pause uses explicit playback commands only. `AudioEffectsCurrentTrack` adapts feature state and callbacks to the shared `MCompactPlayer`; navigation stays in the feature-owned trailing action.

Audio-effect sections use theme surfaces and section text directly. Preset dialogs reuse `MConfirmDialog` and `MTextInputDialog` from `core:common-ui`; the input dialog's optional `supportingContent` slot preserves the overwrite choice. Numeric display formatting belongs to the feature UI `formatter` package.

The Hangeffektek / Audio effects shortcut appears below the complete Now Playing card and above the upcoming queue. `CurrentTrackEvent.AudioEffectsClicked` produces `CurrentTrackEffect.NavigateToAudioEffect(guildId)`; the CurrentTrack route delegates to the shared Navigation3 back stack and opens `AppRoute.AudioEffect`. Back navigation uses `AudioEffectsEvent.BackClicked` and `AudioEffectsEffect.NavigateBack`. The editor shares the application top bar and retains the bottom navigation; it is not a main tab and has no floating action button. Basic settings include Original, Bass Light, Bass Heavy and Nightcore profiles, a four-level bass selector, and speed/pitch from 0.5 to 2.0. Advanced settings include all 15 EQ bands and Karaoke, Rotation, Channel mix, Low-pass, Tremolo, Vibrato, Distortion and Rate. Each advanced card edits the primary parameter exposed by the editor and preserves its other confirmed parameters.

Draft edits remain local until Apply. Preset selection and reset submit immediately. Entry reads the persisted snapshot with GET only; it never enqueues a bot command. Explicit refresh synchronizes with the player; mutations wait for terminal command events and reload the acknowledged snapshot. Polling reads stored snapshots every five seconds without overwriting dirty drafts. Timeout requires explicit synchronization. Commands retain existing same-voice-channel authorization and node capability validation. Custom presets are guild-local, paged ten at a time; save/delete require Manage Server permission, overwriting is explicit, and deletion is confirmed. Drafts can be saved without applying them to the player. Output volume and 24/7 mode are not stored in presets.

## Subfolders

### presentation/

ViewModels and UI contracts independent of the editor's Compose rendering.

**Files:**

- `AudioEffectsViewModel.kt`
- `state/AudioEffectsUiState.kt`
- `AudioEffectsEvent.kt`
- `AudioEffectsEffect.kt`
- `model/AudioEffectControl.kt`
- `mapper/AudioEffectsProfileMapper.kt`
- `di/AudioEffectModule.kt`

**Purpose:** Coordinates local drafts, guild lifetimes, authorized use cases and terminal bot command completion. `commonMain` consumes `core:domain`, `core:model` and the shared `core:presentation` command coordinator. New test cases in `commonTest` are intentionally unimplemented scaffolds.

---

### ui/

Shared Compose rendering and localized editor strings.

**Files:**

- `AudioEffectsRoute.kt`
- `AudioEffectsEffectMessages.kt`
- `AudioEffectsScreen.kt`
- `components/AudioEffectsContent.kt`
- `components/AudioEffectsCurrentTrack.kt`
- `components/AudioEffectsProfiles.kt`
- `components/AudioEffectsBasicSection.kt`
- `components/AudioEffectsAdvancedSection.kt`
- `components/AudioEffectsScrollBar.kt`
- `components/AudioEffectsIntro.kt`
- `components/AudioEffectsTabs.kt`
- `components/AudioEffectsFilters.kt`
- `components/AudioEffectsPresetsSection.kt`
- `components/AudioEffectsActions.kt`
- `components/AudioEffectsDialogs.kt`
- `components/AudioSoundSlider.kt` - Slider and labeled-slider controls, with a local preview.
- `mapper/AudioEffectControlLabelMapper.kt`
- `navigation/AudioEffectEntryProvider.kt`

**Purpose:** Renders state and callbacks in `commonMain`, using semantic theme colors, circular slider thumbs and Hungarian/English resources. The route owns ViewModel observation and feedback. The screen follows the CurrentTrack feature pattern: it configures the top bar and selects initial loading, error or content in a Column with a divider. `AudioEffectsContent` composes lazy-list sections, footer actions and dialogs from focused files under `components`. Pending updates keep existing editable content visible with disabled controls. The screen works without a live repository. The UI module depends on its own presentation module and shared core modules, never on CurrentTrack. `SharedModule` includes `AudioEffectModule` for Koin discovery, and the shared entry provider registers the destination. Android host validation does not validate iOS.

---

## Audio Effects

### AudioEffectsViewModel

**Purpose:** Separates confirmed settings from an editable draft and prevents overlapping submissions or stale guild callbacks.

**Properties:**

```kotlin
val uiState: StateFlow<AudioEffectsUiState>
val effect: SharedFlow<AudioEffectsEffect>
```

**Usage:**

```kotlin
audioEffectsViewModel.open(guildId = guildId)
audioEffectsViewModel.onEvent(event = AudioEffectsEvent.SpeedChanged(value = 1.15f))
audioEffectsViewModel.onEvent(event = AudioEffectsEvent.Apply)
```

**Validation Checks:**

- Debug Android assembly: `./gradlew.bat :androidApp:assembleDebug`.
- New mobile effect tests are scaffolds containing `NotImplementedError`, not functional coverage.
- Backend batch tests cover invalid settings, preservation of unrelated filters and the JSON property contract.
- Live Discord/Lavalink listening and iOS validation require their respective environments.

## Related Components

- [core:network](../../core/network/README.md) - Authorized effect transport and JSON mapping.
- [core:presentation](../../core/presentation/README.md) - Shared durable command coordination.
- [CurrentTrack](../currenttrack/README.md) - Effect-driven entry point.
- [MobileApp](../../README.md) - Application structure and build commands.
