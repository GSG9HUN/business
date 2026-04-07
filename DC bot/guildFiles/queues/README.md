# Guild Queue Files

Legacy queue storage location.

Active queue persistence is database-backed via `IQueueRepository` and `QueueRepository`.

## Contents

- `*.json` files keyed by guild ID.

## Notes

- Existing files may remain from older versions.
- New queue state is persisted to PostgreSQL.

