# Guild Localization Files

This folder stores per-guild localization settings.

## Why This Folder Exists

Guild language selection is persistent state. When a guild chooses Hungarian or English, the bot needs to remember that preference after restart.

This folder is the file-backed storage location for that preference in the bot project. The localization service reads these files when resolving which language should be used for a guild.

## Contents

- `*.json` files keyed by guild ID.

## How It Is Used

Each file represents one guild's selected language. If a guild does not have a file here, the bot falls back to the default language.

This folder is runtime data, not source translation content. Actual translation strings live under `localization/`.

## Maintenance Notes

- These files persist each guild's selected language.
- Missing files fall back to the default language (`eng`).
- Corrupted files currently raise `LocalizationException`; fix or delete the file to restore default loading.

