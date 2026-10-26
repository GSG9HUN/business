# Lifecycle

Coordinates guild player ownership, voice connections, cleanup, and playback recovery through responsibility-specific subfolders.

## Overview

- `Connections` joins and validates players; `Sessions` owns generations, transport draining, and attempt correlation.
- `Cleanup` settles local, remote, and persistence outcomes; `Recovery` handles gateway and uncertain-start evidence.
- `PersistentVoice` shares a guild gate between saved settings, periodic recovery, and explicit leave.
- `Models` contains lifecycle data owned by the bot, including SDK-dependent session identities.
- Join and cleanup deadlines are configured by `DC bot/Configuration/PlaybackLifecycleOptions.cs`.

## Subfolders

### Connections/

Resolve or create a player and wait for validated readiness without abandoning late SDK completion.

**Files:**

- `PlayerConnectionService.cs`
- `PlayerConnectionRetryPolicy.cs`
- `README.md`

**Purpose:** Resolve or create a player and wait for validated readiness without abandoning late SDK completion.

---

### Sessions/

Keep player generations, reservations, attempt state, database admission, and transport draining coordinated.

**Files:**

- `GuildPlaybackSessions.cs`
- `README.md`

**Purpose:** Keep player generations, reservations, attempt state, database admission, and transport draining coordinated.

---

### Cleanup/

Confirm remote disconnection and settle local subscriptions and persistence before releasing ownership.

**Files:**

- `PlaybackCleanupService.cs`
- `DisconnectedPlayerCleanupService.cs`
- `README.md`

**Purpose:** Confirm remote disconnection and settle local subscriptions and persistence before releasing ownership.

---

### Recovery/

Recover non-persistent gateway connections and reconcile uncertain playback using authenticated remote evidence.

**Files:**

- `DiscordPlaybackRecoveryService.cs`
- `PlaybackRecoveryService.cs`
- `PlaybackRemoteProbe.cs`
- `README.md`

**Purpose:** Recover non-persistent gateway connections and reconcile uncertain playback using authenticated remote evidence.

---

### PersistentVoice/

Maintain enabled voice connections without requiring listeners, restore confirmed checkpoints, and honor explicit leave.

**Files:**

- `PersistentVoiceGate.cs`
- `PersistentVoiceLeaveService.cs`
- `PersistentVoiceRecoveryService.cs`
- `TwentyFourSevenService.cs`
- `README.md`

**Purpose:** Maintain enabled voice connections without requiring listeners, restore confirmed checkpoints, and honor explicit leave.

---

### Models/

Share session identities, reservations, cleanup results, and recovery inputs without mixing them with service implementations.

**Files:**

- `ControlReservation.cs`
- `LeaveResult.cs`
- `PlaybackResumeOptions.cs`
- `PlaybackSession.cs`
- `VoiceRecoveryTarget.cs`
- `VoiceRecoveryWindow.cs`
- `README.md`

**Purpose:** Share session identities, reservations, cleanup results, and recovery inputs without mixing them with service implementations.

---

## Ownership and Recovery Rules

- A session is valid only while its player reference and generation remain current.
- Every acquired control reservation or persistent voice semaphore must be released exactly once.
- Leave invalidates ownership before draining admitted transports and database work.
- A cancelled caller may stop waiting; lifecycle-owned cleanup must continue and its outcome must remain observed.
- A missing SDK player does not prove that remote playback stopped. Uncertain starts stay fenced until evidence settles them.
- Enabled persistent voice retains the current-track checkpoint during cleanup. Explicit leave persists disabled mode first and closes the batch after successful cleanup.
- The persistent voice worker checks every five seconds. Restoration uses the last confirmed position and is approximate; non-seekable sources restart without seeking.
- The application explicitly starts workers in `BotApplication.RunAsync`; DI registration alone does not start them.

## Related Components

- `DC bot/Service/Music/README.md` - Parent music workflow and neighboring features.
- `DC bot/Configuration/PlaybackLifecycleOptions.cs` - Join and cleanup deadlines.
- `DC bot/Service/Music/Playback/` - Owned track dispatch, controls, and current-track persistence.
- `DC bot/Service/Music/Events/` - Correlated SDK track events and queue advancement.
- `DC bot/Interface/Service/Music/Lifecycle/` - Player connection service contract.
- `DC bot/Startup/DependencyInjection/MusicServiceCollectionExtensions.cs` - Lifecycle component registration.
- `DC bot/Startup/BotApplication.cs` - Explicit worker lifetime and shutdown coordination.
