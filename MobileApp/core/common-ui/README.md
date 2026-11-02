# common-ui

Shared Compose components, design-system controls, and UI integration for the
Android and iOS applications.

## Overview

- Reusable controls, music-search sheets, track item rows, and feedback components.
- Top-bar and floating-action-button configuration.
- Shared queue drag handling and edge scrolling.
- Material 3 light, dark, and system-following themes.

The module depends on `core:common`, `core:presentation`, and `core:model`. Feature UI modules consume its Compose components; platform adapters remain in their Android or iOS source sets.

## Subfolders

### src/commonMain/

Shared Compose components, theme palettes, resources, biometric UI contracts, and reorder behavior.

**Files:**

- [`MTrackItemRow.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/commonui/components/MTrackItemRow.kt)
- [`MelodiasMarioTheme.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/commonui/designsystem/theme/MelodiasMarioTheme.kt)
- [`AppLockHost.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/commonui/biometric/AppLockHost.kt)

**Purpose:** Shared Compose components, theme palettes, resources, biometric UI contracts, and reorder behavior.

---

### src/androidMain/

Android biometric, lifecycle, feedback, and playback-notification adapters.

**Files:**

- [`BiometricAuthenticator.android.kt`](src/androidMain/kotlin/com/dc/melodiasmario/core/commonui/biometric/BiometricAuthenticator.android.kt)
- [`PlaybackNotificationController.android.kt`](src/androidMain/kotlin/com/dc/melodiasmario/core/commonui/playbacknotification/PlaybackNotificationController.android.kt)

**Purpose:** Android biometric, lifecycle, feedback, and playback-notification adapters.

---

### src/iosMain/

Native iOS biometric, lifecycle, feedback, and playback-notification adapters.

**Files:**

- [`BiometricAuthenticator.ios.kt`](src/iosMain/kotlin/com/dc/melodiasmario/core/commonui/biometric/BiometricAuthenticator.ios.kt)
- [`PlaybackNotificationController.ios.kt`](src/iosMain/kotlin/com/dc/melodiasmario/core/commonui/playbacknotification/PlaybackNotificationController.ios.kt)

**Purpose:** Native iOS biometric, lifecycle, feedback, and playback-notification adapters.

---

## Components

### MelodiasMarioTheme and MText

**Purpose:** `designsystem/theme/MelodiasMarioTheme.kt` is the single palette source.
Components read roles directly from `MaterialTheme.colorScheme`; they do not
cache an entire palette in a local `colors` variable or use a parallel token API.

| UI purpose | Material role |
| --- | --- |
| Main text and icons on a surface | `onSurface` |
| Supporting text and icons | `onSurfaceVariant` |
| Filled primary controls and their content | `primary`, `onPrimary` |
| Neutral cards and elevated surfaces | `surface`, `surfaceContainer*` |
| Borders and dividers | `outline`, `outlineVariant` |
| Errors and destructive actions | `error`, `errorContainer`, `onErrorContainer` |
| Green success / online status | `tertiaryContainer`, `onTertiaryContainer` |

`MText` defaults to `onSurfaceVariant`. Supply the correct content role for
primary controls or other colored containers. In non-composable drawing
callbacks, pass a color obtained in composable scope rather than reading
`MaterialTheme` inside the drawing callback.

Both palettes define surface-container and status colors explicitly. System
mode observes the device theme through `isSystemInDarkTheme()`. Preview
annotation constants remain available because annotations require constants.
Existing artwork gradients and warning-toast accents retain their explicit
decorative colors.

**Validation Checks:**

Run the following command from the MobileApp root.

Compile and package the Android app with:

```powershell
.\gradlew.bat :androidApp:assembleDebug '-Pkotlin.incremental=false'
```

The build checks Android compilation and packaging. Visual behavior in both
themes and iOS compilation need their respective runtime / platform checks.

---

### AppLockHost and BiometricLockSetting

**Purpose:** The biometric package contains native authenticator and lifecycle adapters, an
application lock host, a lock screen, localized errors, and a stateless profile toggle.
AppLockHost supplies the controller and authenticator to routes. ProfileRoute passes
controller state through the screen to BiometricLockSetting. Visual components have
theme-wrapped previews that do not open native prompts or require DI.

See [the shared lock controller](../presentation/src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/biometric/AppLockController.kt).

---

### PlaybackNotificationController and ReorderDragContainer

**Purpose:** PlaybackNotificationController exposes notification startup. The Android foreground service owns notification STOP actions and task-removal cleanup. ReorderDragContainer calculates the drop target from visible list geometry; there is no separate unused index helper.

---

### MTrackItemRow

**Purpose:** MTrackItemRow in components/ is shared by QueueScreen, CurrentTrackScreen, and PlaylistTrackScreen, including drag overlays. It displays trackNumber, artwork, title, artist and duration, a remove button, and a drag handle. enabled controls interaction; reorderEnabled separately gates move accessibility actions and local dragging. Container-managed dragging uses dragEnabled = false to avoid competing gesture handlers. Queue ordering is unchanged; playlist ordering remains disabled until backend support exists.

---

### MPlaylistArtwork and MArtwork

**Purpose:** `components/MPlaylistArtwork.kt` prefers the API image URL and uses bundled vector covers selected by `PlaylistArtworkTheme.fromName` when the image is missing, loading, or failed. Night Drive uses a neon city/road image, Chill uses a sunset/waves image, and Workout uses a lightning image. Other names retain the gradient/music icon. Both playlist cards and detail headers use this component. `MArtwork.fallbackImage` accepts an optional full-size local fallback without changing queue artwork defaults.

---

### MMoreAction and shared UI contracts

**Purpose:** UI callbacks declare descriptive input parameter names. Components use named arguments, explicit imports, and KDoc for their contracts. `MMoreAction`, `MelodiasMarioThemeMode`, and `FloatingActionButtonIcon` each have a separate file. IDE previews remain private helpers.

---

### queueBatchItems and MSwitchButton

**Purpose:** `components/QueueBatchItems.kt` renders batch headers and read-only history while preserving the zero-based indexes of waiting tracks passed to its `key` and `itemContent` callbacks. Playing occurrences use the primary-container background; completed and skipped occurrences are dimmed. History rows have no queue mutation callbacks.

The switch thumb in `MSwitchButton` reads `MaterialTheme.colorScheme.onPrimary` instead of a fixed color. Callbacks name their inputs, and fluent chains keep one subsequent call per line.

---

## Related Components

- [core:presentation](../presentation/README.md): shared presentation contracts and controllers.
- [core:model](../model): shared application models.
- Feature UI modules consume these Compose components.
