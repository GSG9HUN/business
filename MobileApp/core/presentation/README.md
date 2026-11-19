# presentation

Shared presentation contracts and coordination for Kotlin Multiplatform features.

## Overview

- Resource runners update UI state and emit effects for ordinary and bot-command requests.
- `BotCommandCoordinator` matches accepted commands to realtime completion, failure, or timeout.
- Search controllers and reducers manage capabilities, queries, selection, and pagination.
- Queue helpers coordinate track additions, removal, movement, clearing, and shuffling.
- The application-lock controller coordinates authentication and lock-state transitions.

`commonMain` contains platform-independent contracts; `commonTest` contains shared tests. Gradle configures Android, iOS device (`iosArm64`), and iOS simulator (`iosSimulatorArm64`) targets. The module exposes `core:common`, `core:model`, and coroutine APIs and has no dependency on Compose, `core:common-ui`, or `core:domain`.

## Subfolders

### src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/command/

Command locking, completion outcomes, and realtime command coordination.

**Files:**

- [`BotCommandCoordinator.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/command/BotCommandCoordinator.kt)
- [`BotCommandResult.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/command/BotCommandResult.kt)

**Purpose:** Command locking, completion outcomes, and realtime command coordination.

---

### src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/music/

Music-input models, state contracts, search coordination, and reducers.

**Files:**

- [`MAddMusicMode.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/music/model/MAddMusicMode.kt)
- [`MMusicSearchKind.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/music/model/MMusicSearchKind.kt)

**Purpose:** Music-input models, state contracts, search coordination, and reducers.

---

### src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/queue/

Queue-state contracts and local track reordering.

**Files:**

- [`TrackQueueReducer.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/queue/TrackQueueReducer.kt)
- [`TrackQueueUiState.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/queue/TrackQueueUiState.kt)

**Purpose:** Queue-state contracts and local track reordering.

---

### src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/playbackmonitor/

Lifecycle-bound observation of shared playback snapshots.

**Files:**

- [`PlaybackMonitorObserver.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/playbackmonitor/PlaybackMonitorObserver.kt)

**Purpose:** Lifecycle-bound observation of shared playback snapshots.

---

### src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/biometric/

Application-lock state, outcomes, and coordination.

**Files:**

- [`AppLockController.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/biometric/AppLockController.kt)
- [`AppLockError.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/biometric/AppLockError.kt)

**Purpose:** Application-lock state, outcomes, and coordination.

---

### src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/formatter/

Shared display formatting for track durations.

**Files:**

- [`TrackDurationFormatter.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/formatter/TrackDurationFormatter.kt)

**Purpose:** Shared display formatting for track durations.

---

### src/commonTest/

Shared presentation tests, including implemented behavior checks and explicit test scaffolds.

**Files:**

- [`BotCommandCoordinatorTest.kt`](src/commonTest/kotlin/com/dc/melodiasmario/core/presentation/command/BotCommandCoordinatorTest.kt)

**Purpose:** Shared presentation tests, including implemented behavior checks and explicit test scaffolds.

---

## Components

### BotCommandCoordinator

**Purpose:** Matches command, guild, and optional user IDs, retains early terminal events, and reports completion, failure, or a 30-second timeout. Cancellation propagates.

**Methods:**

- `awaitCompletion` waits for a terminal result.
- `handleCompletion` dispatches completion and feature outcome callbacks.

---

### MusicSearchController

**Purpose:** Coordinates music-search state, provider selection, result kinds, query changes, pagination, and cancellation of obsolete requests.

---

### TrackQueueReducer

**Purpose:** Creates immutable queue state after local reordering.

**Validation Checks:**

Equal or out-of-range indexes leave the state unchanged.

---

### PlaybackMonitorObserver

**Purpose:** Starts one lifecycle-owned playback observation job and cancels it when stopped.

---

### AppLockController

**Purpose:** Coordinates local application-lock state and authentication results without depending on Compose or native prompt APIs.

---

### Shared presentation contracts

**Purpose:** Callbacks name their input parameters, and calls use named arguments wherever Kotlin supports them. Public contracts use KDoc and explicit types. Existing implemented tests are preserved; new test cases remain `NotImplementedError()` scaffolds unless implementation is explicitly requested.

**Validation Checks:**

Run `.\gradlew.bat :androidApp:assembleDebug` from the MobileApp root to check integration compilation and packaging. This does not run behavioral tests or validate iOS. Test scaffolds throw `NotImplementedError()` and do not provide behavior coverage.

---

## Related Components

- `core:common` and `core:model` provide shared resource contracts and domain models.
- Feature presentation modules supply operations and feature-specific callbacks.
- `core:common-ui` renders the presentation contracts. This module has no Compose, common-ui, or domain dependency.
