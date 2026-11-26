# ui

Compose audio-effect screen, controls, localized resources and Navigation3 entry registration.

## Overview

Preset saving uses `SaveAudioEffectPresetUseCase` and `AudioEffectRepository.saveAudioEffectPreset`, which POST a typed `name`/`commands`/`overwrite` body to `/guilds/{guildId}/playback/audio-effects/presets`. `ApplyAudioEffectsUseCase` continues to use `/guilds/{guildId}/playback/audio-effects`; saving never invokes that use case. The unapplied draft remains editable after saving.

Custom presets can save the edited draft without an earlier Apply and without changing live audio. Mobile sends the name, overwrite choice and draft setting edits to the dedicated preset endpoint; the API constructs the internal save operation, and the bot composes a definition from the authoritative snapshot under the existing save reservation, rechecks management/voice permissions, and persists it without committing filters to Lavalink. The dirty draft remains available for a later Apply. The custom-preset page uses a server callout and compact preset rows and ends with the create button and muted volume/24-7 exclusion note.

The EQ scrollbar shares the band row's measured scroll state and supports drag, track taps, accessible progress changes and right-to-left layouts. Hungarian/default and English resources contain matching keys and placeholder contracts; effect/profile names such as Nightcore and Equalizer intentionally remain identical.

Advanced sound includes a guild/voice-room top-bar subtitle separated by a Unicode middle dot, a cyan overline with 24sp headline and muted description, underline tabs, a cyan EQ reset at top right, the 15-band caption and visible horizontal-scroll indicator/hint. Filter rows open local-draft editing dialogs instead of switches. Only Apply advanced settings and Save as custom preset appear in the advanced footer. Keeping or cancelling a filter popup never sends a bot command; Apply commits the draft.

The audio-effect Screen has no Scaffold; the shared navigation layer owns page chrome and insets. Its Route collects effects sequentially and delegates localized feedback to `AudioEffectsEffectMessages` and the application toast host. Dialog-only form content stays in `AudioEffectsDialogs.kt`, related slider composables share `AudioSoundSlider.kt`, and substantial editor sections remain separate. Each visual component has previews beside its implementation, without a live ViewModel or network.

The sound editor includes live mini-player header, large icon profile tiles with a selected check mark, green confirmed-profile badge, right-aligned multiplier values, full-width icon actions, and the actual confirmed output-volume note. The basic page links to advanced sound; advanced back returns to basic settings, and basic back leaves without issuing a bot command. Mini-player play/pause uses explicit playback commands only. `AudioEffectsCurrentTrack` adapts feature state and callbacks to the shared `MCompactPlayer`; navigation stays in the feature-owned trailing action.

Audio-effect cyan accents (profile icons, multiplier values and the advanced-sound link icons) use the shared `MaterialTheme.colorScheme.secondary` defined for light and dark modes in `MelodiasMarioTheme.kt`. No feature-local cyan color is defined. Audio-effect labels use the shared `MText` component, preserving their inherited content colors, text styles, alignment and line heights.

Audio-effect sections use theme surfaces and section text directly. Preset dialogs reuse `MConfirmDialog` and `MTextInputDialog` from `core:common-ui`; the input dialog's optional `supportingContent` slot preserves the overwrite choice. Numeric display formatting belongs to the feature UI `formatter` package.

The module uses `commonMain` for shared Android/iOS functionality and depends on its own `feature:audioeffect:presentation`, `core:common`, `core:common-ui` and `core:model`; the module does not import CurrentTrack. Android host compilation does not validate iOS; iOS framework builds require macOS/Xcode.

## Subfolders

### src/

Platform-independent implementation and configured source sets.

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
- `formatter/AudioEffectValueFormatter.kt`
- `navigation/AudioEffectEntryProvider.kt`

**Purpose:** Owns the ui responsibilities of the audio-effect feature. Test scaffolds remain unimplemented and must not be counted as passing behavioral tests.

---

## Module Configuration

### build.gradle.kts

**Purpose:** Configures the Android library and `iosArm64` / `iosSimulatorArm64` frameworks using the shared version catalog. The new module is included by the application through `shared`.

**Validation Checks:**

- `./gradlew.bat :androidApp:assembleDebug` verifies Android compilation and packaging of both modules.

## Related Components

- [audioeffect](../README.md) - Feature behavior, source ownership and navigation.
- [MobileApp](../../../README.md) - Application structure and build commands.
