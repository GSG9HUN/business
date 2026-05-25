# Guild Localization Files

This folder stores per-guild localization settings.

## Contents

- `*.json` files keyed by guild ID.

## Notes

- These files persist each guild's selected language.
- Missing files fall back to the default language (`eng`).
- Corrupted files currently raise `LocalizationException`; fix or delete the file to restore default loading.

