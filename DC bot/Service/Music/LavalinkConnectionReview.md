# Lavalink / voice connection review (2026-10-07)

## Observed disconnect

The existing `DC bot/logs` capture contains this sequence (lines 3869-3934):

1. Discord sends gateway opcode 7 (`RECONNECT`).
2. DSharpPlus reconnects and identifies, then receives a new `READY` session.
3. Lavalink reports a voice `WebSocketClosedEvent`: code 4014, `byRemote: true`, reason `Disconnected.`.
4. The player reports `connected: false`; the Lavalink node continues sending statistics.

This capture shows a Discord voice-session loss during a gateway reconnect, not loss of the bot-to-Lavalink node connection. It does not establish why the Discord client identified instead of resuming, or prove that every reported disconnect has this cause. This is an older capture, not an observed reproduction of the current deployment.

## Corrected checks

- Service startup was used as a permanent readiness flag. Readiness is now checked on each call; a failed readiness wait does not start the already-started service a second time.
- Post-join checks previously ran at 0/200/400/600/800 ms and then returned an error. They now allow readiness checks through 10 seconds, without sleeping after the final failed check.
- Concurrent join/play requests could run stale-player cleanup while another request was joining. Cleanup, join and validation are serialized per guild. Existing-player validation also waits for this join, then checks node readiness.
- A missing guild player incorrectly produced `LavalinkError`. It now produces `BotIsNotConnectedError`; node/join exceptions retain the Lavalink error path.

## Follow-up fixes

- Stale-player cleanup observes the same player every 500 ms for 10 seconds before disconnecting. Recovery, replacement/removal, and cancellation stop cleanup without disconnecting the player. This is a bounded grace period, not a guarantee against arbitrarily long reconnects.
- `ExecuteWithExistingPlayerAsync` holds the guild lock through validation and the entire supplied operation. Leave uses it for handler cleanup, stop, disconnect and state persistence. The lock is released in `finally`, including operation failures; no nested acquisition occurs.
- Connected/disconnected status persistence failures are logged separately from transport failures. A successful voice join remains successful when the status write fails; cleanup can proceed to rejoin after a failed status write. The persisted UI state can remain stale until a subsequent successful update.
- Gateway close code 4014 is logged as disallowed intents. It is not interpreted using the separate voice close-code table: https://docs.discord.com/developers/topics/opcodes-and-status-codes

## Validation and limits

The final full unit suite passed: 926 tests, 0 failures. Follow-up coverage includes transient recovery, replacement, cancellation, persistent stale cleanup, join/operation serialization with success and failure, persistence failure classification, and gateway close-code logging. Regression tests cover readiness rechecks, readiness failure after successful startup, delayed voice readiness beyond the former retry window, concurrent joins and existing-player requests, cancellation during retries, and missing-player classification.

These changes improve command-time connection checks. They do not implement automatic voice rejoin or playback restoration after a Discord session replacement. An intentional kick/leave must not be blindly undone. To diagnose a remaining live disconnect, correlate gateway RECONNECT/RESUMED/READY events, bot voice-state transitions, and Lavalink voice close/player-update events from the same incident. No live bot session was started or interrupted during this review.
