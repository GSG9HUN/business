# presentation

Shared Kotlin Multiplatform presentation code for the Android and iOS applications.

## Overview

The module owns shared command coordination, search state and controllers, queue reducers, presentation models, and display mapping. It has no Compose plugin, Compose imports, or dependency on common-ui, domain, data, or network.

Use cases remain in feature ViewModel constructors. Shared controllers and handlers receive their invoke functions as callbacks.

## Dependencies

- core:common supplies Resource and shared utility types.
- core:model supplies business models such as Track and BotControlCommand.
- kotlinx.coroutines supplies scopes, flows, and jobs.

These dependencies use api because their types occur in public presentation contracts.

Consumers include core:common-ui, feature presentation modules, and application composition modules. common-ui exports core:presentation because its public Compose component parameters include the shared state and presentation models. Modules that import presentation declarations also declare their dependency directly.

## Subfolders

Paths below are relative to src/commonMain/kotlin/com/dc/melodiasmario/core/presentation.

### Root package

**Files:** MviEffect.kt, ResourceActionRunner.kt, BotCommandActionRunner.kt, BotCommandBackgroundRunner.kt, TrackQueueCommandHandler.kt, BotCommandRealtimeObserver.kt.

**Purpose:** Shared effects, Resource flow collection, queue command execution, and terminal command-event observation.

---

### command/

**Files:** CommandUiState.kt, CommandLock.kt, PendingBotCommandTracker.kt.

**Purpose:** Screen-independent command locking, command-id matching, and timeout tracking.

---

### queue/

**Files:** TrackQueueUiState.kt, TrackQueueReducer.kt.

**Purpose:** Immutable queue-list updates with index validation.

---

### music/search/

**Files:** MusicSearchUiState.kt, MusicSearchController.kt, MusicSearchReducer.kt.

**Purpose:** Shared search orchestration and state transformations, including capabilities, debounce, pagination, and result selection.

---

### music/model/ and music/state/

**Files:** MAddMusicMode.kt, MMusicSearchKind.kt, MMusicSearchProviderUi.kt, MMusicSearchResultUi.kt, MAddMusicSheetState.kt.

**Purpose:** Compose-independent models consumed by feature presentation and reusable UI components. Compose-aware MAddMusicLabels remains in core/common-ui.

---

### music/ and formatter/

**Files:** MusicSearchUiMappers.kt, TrackDurationFormatter.kt.

**Purpose:** Business-to-presentation mapping and duration formatting without depending on Compose.

## Validation

From the MobileApp directory:

~~~powershell
.\gradlew.bat :androidApp:assembleDebug '-Pkotlin.incremental=false'
~~~

Android build verified with this invocation: BUILD SUCCESSFUL (51 seconds, 446 actionable tasks). The invocation disables incremental compilation without changing project properties. No behavior tests were added for this package/module relocation. Existing presentation test scaffolds remain unchanged.

## Related Components

- core/common: shared Resource and utility types.
- core/model: business data models.
- core/common-ui: Compose components, themes, localized labels, and platform UI integrations.
- feature/currenttrack/presentation and feature/queue/presentation: feature events, effects, ViewModels, and command adapters.

### biometric/

**Files:** AppLockController.kt, AppLockUiState.kt, AppLockError.kt, BiometricResult.kt.

**Purpose:** Coordinate the device-local application lock, initial settings load,
authenticated setting changes, local sign-out, and background timeout. Native prompts
and persistence are supplied through callbacks. The controller has no Compose or
platform dependency and owns no coroutine scope or timer job.

See [the implementation walkthrough](../../docs/biometric-app-lock.md).
