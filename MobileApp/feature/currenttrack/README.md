# currenttrack

Guild playback presentation and Compose controls for Now Playing, seeking, output volume and the upcoming queue.

## Overview

The Hangeffektek / Audio effects shortcut is outside the complete Now Playing card, below output volume and above the upcoming queue. The screen submits `CurrentTrackEvent.AudioEffectsClicked`; the ViewModel emits `CurrentTrackEffect.NavigateToAudioEffect` with the selected guild ID. The route forwards the effect to the shared Navigation3 back stack, which opens the independent [audioeffect feature](../audioeffect/README.md). CurrentTrack has no Gradle dependency on that feature and does not own the editor's ViewModel, resources or visibility state.

## Subfolders

### presentation/

Playback state, events, effects and command handling.

**Files:**

- `CurrentTrackViewModel.kt`
- `CurrentTrackEvent.kt`
- `CurrentTrackEffect.kt`
- `state/CurrentTrackUiState.kt`

**Purpose:** Coordinates shared domain operations, playback monitoring and one-time navigation intentions. `commonMain` contains platform-independent presentation. Existing seek and volume tests remain scaffolds.

---

### ui/

Composable playback rendering and navigation entry registration.

**Files:**

- `CurrentTrackRoute.kt`
- `CurrentTrackScreen.kt`
- `components/NowPlayingCard.kt`
- `components/AudioEffectsNavigationButton.kt`
- `presentation/navigation/CurrentTrackEntryProvider.kt`

**Purpose:** Renders playback state and translates navigation effects into callbacks supplied by the shared application layer. Hungarian and English shortcut labels remain owned by this module.

---

## Navigation

### CurrentTrackEffect.NavigateToAudioEffect

**Purpose:** Carries the guild selected when the navigation event was handled, without importing the target feature.

**Usage:**

```kotlin
onEvent(CurrentTrackEvent.AudioEffectsClicked)
```

**Validation Checks:**

- `./gradlew.bat :androidApp:assembleDebug` checks the shared module graph and Android packaging.
- Navigation test scaffolds record intended behavior and do not provide implemented coverage.

## Related Components

- [audioeffect](../audioeffect/README.md) - Independent sound editor and presentation.
- [core:presentation](../../core/presentation/README.md) - Shared command coordination.
- [MobileApp](../../README.md) - Application structure and build commands.
