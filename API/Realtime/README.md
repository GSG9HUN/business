# Realtime

This folder contains the API realtime gateway used by the MobileApp.

## Purpose

Persistence repositories publish PostgreSQL notifications when playback, queue, guild bot status, or bot-control command state changes. The API listens to those notifications and forwards authorized SignalR events to mobile clients.

## Root Files

### MobileUpdatesHub.cs

Authenticated SignalR hub for mobile realtime subscriptions.

### MobileUpdateGroups.cs

Central group-name helpers for user-scoped and guild-scoped SignalR subscriptions.

## Subfolders

### Events/

SignalR event payload records.

### Listening/

PostgreSQL listener and options for receiving database notifications.

### Publishing/

SignalR publishing services.

### Snapshots/

Snapshot provider used to attach REST-equivalent state to realtime events.

## Flow

```text
Repository state change
  -> PostgreSQL NOTIFY
  -> MobileRealtimePostgresListener
  -> snapshot provider
  -> realtime publisher
  -> MobileUpdatesHub group
  -> MobileApp refresh/snapshot handling
```

## Playback Notes

- Realtime events should carry snapshots that match REST read endpoints.
- Mobile clients should treat events as refresh/snapshot triggers, not as the only source of truth.
