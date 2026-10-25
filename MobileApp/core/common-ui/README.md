# common-ui

Shared Compose components, design-system controls, and UI integration for the
Android and iOS applications.

## Overview

- Reusable controls, music-search sheets, queue cards, and feedback components.
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
