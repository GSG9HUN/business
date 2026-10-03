# API Repositories

This folder is currently reserved.

## Purpose

API-specific repository adapters can live here if the API later needs persistence behavior that does not belong in the shared persistence project.

## Current Direction

Most persistence should stay behind contracts and implementations in:

- `DC bot.Contracts/Interface/Service/Persistence/`
- `DC bot.Persistence/Repositories/`

## Notes

- Prefer existing persistence contracts before adding API-owned repositories.
- Do not duplicate EF Core queries here when a shared repository already exists.
