# Queue Configurations

This folder contains EF Core mappings for queue entities.

## Why This Folder Exists

Queue queries depend heavily on guild, state, and ordering. The EF mapping here defines how queue rows are stored and which indexes support the repository's common operations.

This is also where enum-to-column conversion is kept, so repository code can use `QueueItemState` instead of raw numeric values.

## Files

### GuildQueueItemConfiguration.cs

Maps queue item columns, indexes, relationships, and state conversion.

## What To Keep Here

- queue table and column names
- queue item state conversion
- indexes for queued item lookups and ordering
- guild relationship mapping

## Maintenance Notes

- Queue item state is converted to the `short` database column.
- Queue ordering and state indexes should stay aligned with repository query patterns.
