# Queue Repositories

This folder contains queue repository implementation and queue-specific helpers.

## Why This Folder Exists

Queue persistence is more than CRUD. It has ordering rules, state transitions, max-size rules, and an atomic claim operation used by playback. This folder keeps that behavior in one place.

## Files

### QueueRepository.cs

Implements `IQueueRepository`.

Responsibilities:

- read queued items
- enqueue one or many tracks
- reorder queued items
- update queue item position and state
- claim the next queued item atomically

### QueueItemMapper.cs

Internal mapper from EF queue entities to queue contract records.

### QueueClaimService.cs

Internal service that performs the transactional queue claim workflow.

### PostgreSqlQueueClaimSql.cs

Provider-specific SQL used for atomic queue claiming.

## Flow Context

Commands enqueue and reorder tracks. Playback claims the next queued item and marks it as playing in one transaction. Completed or skipped tracks move to terminal states so history and previous-track behavior can still work.

## Maintenance Notes

- Queue claiming uses PostgreSQL locking behavior.
- Keep provider-specific SQL isolated in `PostgreSqlQueueClaimSql`.
- Keep queue ordering updates transactional.
