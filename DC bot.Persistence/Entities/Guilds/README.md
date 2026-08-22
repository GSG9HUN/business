# Guild Entities

This folder contains guild-level EF Core entities.

## Why This Folder Exists

Guild data is the root of the persistence model. Most feature tables are guild-scoped, so this folder contains the entity that represents a known Discord guild and the entity used to audit premium state changes.

## Files

### GuildDataEntity.cs

Root guild table entity.

Stores:

- guild id
- premium status
- optional premium expiry
- navigation collections for guild-owned data

### GuildPremiumAuditEntity.cs

Premium status history entity.

## What The Entities Represent

`GuildDataEntity` is the anchor row for guild-owned data. `GuildPremiumAuditEntity` records premium status changes when audit history is needed.

## Maintenance Notes

- Other feature entities reference guild data through `GuildId`.
- Active premium updates currently go through `GuildDataRepository`.
