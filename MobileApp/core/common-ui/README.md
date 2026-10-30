# common-ui

Shared Compose components, design-system controls, and UI integration for the
Android and iOS applications.

## Overview

- Reusable controls, music-search sheets, track item rows, and feedback components.
- Top-bar and floating-action-button configuration.
- Shared queue drag handling and edge scrolling.
- Material 3 light, dark, and system-following themes.

## Theme colors

`designsystem/theme/MelodiasMarioTheme.kt` is the single palette source.
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

## Related modules

- `core/presentation`: shared presentation contracts and controllers.
- `core/model`: shared application models.
- Feature UI modules consume these Compose components.

## Validation

Compile and package the Android app with:

```powershell
.\gradlew.bat :androidApp:assembleDebug '-Pkotlin.incremental=false'
```

The build checks Android compilation and packaging. Visual behavior in both
themes and iOS compilation need their respective runtime / platform checks.

## Biometric UI

The biometric package contains native authenticator and lifecycle adapters, an
application lock host, a lock screen, localized errors, and a stateless profile toggle.
AppLockHost supplies the controller and authenticator to routes. ProfileRoute passes
controller state through the screen to BiometricLockSetting. Visual components have
theme-wrapped previews that do not open native prompts or require DI.

See [the implementation walkthrough](../../docs/biometric-app-lock.md).

## Playback and reorder lifecycle

PlaybackNotificationController exposes notification startup. The Android foreground service owns notification STOP actions and task-removal cleanup. ReorderDragContainer calculates the drop target from visible list geometry; there is no separate unused index helper.

## Track item rows

MTrackItemRow in components/ is shared by QueueScreen, CurrentTrackScreen, and PlaylistTrackScreen, including drag overlays. It displays trackNumber, artwork, title, artist and duration, a remove button, and a drag handle. enabled controls interaction; reorderEnabled separately gates move accessibility actions and local dragging. Container-managed dragging uses dragEnabled = false to avoid competing gesture handlers. Queue ordering is unchanged; playlist ordering remains disabled until backend support exists.

## Playlist Fallback Artwork

`components/MPlaylistArtwork.kt` prefers the API image URL and uses bundled vector covers selected by `PlaylistArtworkTheme.fromName` when the image is missing, loading, or failed. Night Drive uses a neon city/road image, Chill uses a sunset/waves image, and Workout uses a lightning image. Other names retain the gradient/music icon. Both playlist cards and detail headers use this component. `MArtwork.fallbackImage` accepts an optional full-size local fallback without changing queue artwork defaults.
