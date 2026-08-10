# Shared Configurations

This folder contains EF Core configuration helpers shared by multiple feature mappings.

## Why This Folder Exists

Some EF mapping rules are not owned by one feature. Guild id mapping is a good example: many tables store Discord guild IDs and they should all be configured consistently.

Putting shared mapping helpers here avoids copy-pasting the same configuration code into every feature configuration.

## Files

### GuildIdPropertyBuilderExtensions.cs

Shared helper for configuring guild id properties.

## When To Add Something Here

Add helpers here only when at least two feature configurations need the same EF mapping behavior. If a rule is specific to one table, keep it inside that feature's configuration file.

## Maintenance Notes

- Keep reusable EF mapping helpers here when they are not owned by one feature.
- Shared helpers should stay small and obvious; avoid hiding feature-specific constraints behind generic helpers.
