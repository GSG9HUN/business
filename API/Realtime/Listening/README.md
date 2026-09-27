# Realtime Listening

This folder contains PostgreSQL notification listening infrastructure.

## Files

### MobileRealtimePostgresListener.cs

Background listener that subscribes to database notification channels and dispatches realtime updates.

### PostgresRealtimeOptions.cs

Configuration options for PostgreSQL realtime listening.

## Notes

- Listener code should not contain HTTP endpoint logic.
- Snapshot creation and SignalR publishing should remain delegated to dedicated services.
