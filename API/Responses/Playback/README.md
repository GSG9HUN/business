# Playback Responses

This folder contains response DTOs for playback state endpoints.

## Why This Folder Exists

Playback commands and playback snapshots have different response needs. Command endpoints may only confirm that a bot-control command was accepted, while future read endpoints may return the current player state.

This folder is reserved for the read/snapshot shape so command acknowledgement responses do not become mixed with player-state DTOs.

## Files

### PlaybackStatusResponse.cs

Reserved for current playback state such as player connection, current track, pause state, and timing.

## Flow Context

When a mobile client asks what is playing right now, the API should return a response model from this folder. When the client asks to pause or skip, the API may return an accepted command result instead.

## Maintenance Notes

- Playback command submission currently returns command acceptance data directly from the handler.
- Rich playback snapshots should be modeled here when player read endpoints are implemented.
