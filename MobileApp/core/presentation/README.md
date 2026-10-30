# presentation

Shared presentation contracts and coordination for Kotlin Multiplatform features.

## Overview

- Resource runners update UI state and emit effects for ordinary and bot-command requests.
- `BotCommandCoordinator` matches accepted commands to realtime completion, failure, or timeout.
- Search controllers and reducers manage capabilities, queries, selection, and pagination.
- Queue helpers coordinate track additions, removal, movement, clearing, and shuffling.
- The application-lock controller coordinates authentication and lock-state transitions.

## Subfolders

### command/

Command locking, completion outcomes, and realtime command coordination.

### music/

Music-input models, state contracts, search coordination, and reducers.

### queue/

Queue-state contracts and local track reordering.

### playbackmonitor/

Lifecycle-bound observation of shared playback snapshots.

### biometric/

Application-lock state, outcomes, and coordination.

## Related Components

- `core:common` and `core:model` provide shared resource contracts and domain models.
- Feature presentation modules supply operations and feature-specific callbacks.
- `core:common-ui` renders the presentation contracts. This module has no Compose, common-ui, or domain dependency.

## Source conventions

Callbacks name their input parameters, and calls use named arguments wherever Kotlin supports them. Public contracts use KDoc and explicit types. Existing implemented tests are preserved; new test cases remain `NotImplementedError()` scaffolds unless implementation is explicitly requested.
