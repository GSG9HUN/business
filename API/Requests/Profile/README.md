# Profile Requests

This folder contains request DTOs for authenticated mobile profile settings endpoints.

## Why This Folder Exists

Profile settings updates are HTTP input from Android. They should stay separate from persistence records because a client update can be partial, while persistence records represent the full saved settings state.

## Files

### UpdateProfileSettingsRequest.cs

Request body for `PATCH /api/profile/settings`.

Fields:

- `LanguageCode` - optional language code
- `Theme` - optional app theme value
- `HapticFeedbackEnabled` - optional haptic feedback toggle
- `SoundEffectsEnabled` - optional sound effects toggle
- `TelemetryEnabled` - optional telemetry toggle

## Maintenance Notes

- Nullable fields mean "leave unchanged".
- The authenticated Discord user ID must come from JWT claims, not the request body.
