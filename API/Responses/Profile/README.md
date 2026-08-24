# Profile Responses

This folder contains response DTOs for the authenticated mobile profile screen.

## Why This Folder Exists

The profile screen needs user identity and app settings in an Android-friendly shape. These DTOs keep that public HTTP response separate from EF entities and repository records.

## Files

### ProfileResponse.cs

Top-level response for `GET /api/profile`.

### ProfileUserResponse.cs

Discord-backed mobile user profile fields.

### ProfileSettingsResponse.cs

Saved app settings fields returned after profile load or settings update.

## Maintenance Notes

- Keep Discord snowflake IDs serialized as strings.
- Do not expose refresh token hashes or persistence-only session fields here.
