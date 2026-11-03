# MobileApp

This is a Kotlin Multiplatform project targeting Android and iOS.

## AI agent conventions

[AGENTS.md](AGENTS.md) defines the instructions agents must read before modifying this project. It covers Kotlin formatting and naming, module boundaries, Koin dependency injection, coroutine and state handling, Compose UI, KDoc, testing, and README maintenance. Argument lists must have no trailing commas, calls must use named arguments wherever Kotlin supports them, and all existing and new function-type declarations throughout the project must name their input parameters descriptively, across every module and source set. New tests are created as test classes and named test-case scaffolds whose bodies contain only `throw NotImplementedError()`; test logic is implemented only when explicitly requested. Existing implemented tests are preserved. Variables, properties, and parameters must identify their specific type and role, such as `authRepository` rather than `repository`; only `uiState` and `effect`, including their backing properties, may retain these short generic names.

## UI theme colors

Compose screens and shared components read semantic colors directly from
`MaterialTheme.colorScheme`. Main text uses `onSurface`, supporting text uses
`onSurfaceVariant`, and content on filled primary controls uses `onPrimary`.
Surfaces use the `surface` / `surfaceContainer*` roles; borders use `outline`
or `outlineVariant`. Error and destructive states use `error` /
`errorContainer`; green success and online indicators use `tertiaryContainer`
with `onTertiaryContainer`.

`MelodiasMarioTheme` defines both palettes and follows the device theme when
System is selected. The former `MelodiasMarioThemeTokens` /
`MelodiasMarioColors` layer has been removed. `MText` keeps
`MaterialTheme.colorScheme.onSurfaceVariant` as its default color; callers
select a different semantic role explicitly when needed. Drawing callbacks
receive colors read in composable scope. Fixed artwork gradients and the
existing warning-toast accent remain explicit decorative colors.

See [core/common-ui/README.md](core/common-ui/README.md) for the shared UI color conventions.

Queue localization resources keep a single entry per key; duplicated button labels were consolidated to their updated short labels.

## Shared presentation

The core:presentation module contains shared command coordination, effects, action runners, queue reducers, search controllers, presentation models, and mappings. It depends on core:common and core:model, with no dependency on common-ui or domain. Compose components and localized labels remain in core:common-ui, which consumes the shared presentation contracts.

See [core/presentation/README.md](core/presentation/README.md) for module responsibilities and dependency direction.

## Queue screen

The queue screen loads the selected guild through the shared playback monitor. It supports manual music input, provider and track/playlist search, pagination and retry, removing tracks, moving tracks, clearing and shuffling the queue, and returning to guild selection.

Queue mutations use command-id tracking and terminal realtime events. A matching terminal event or a terminal state returned in the HTTP response releases the command lock and refreshes the server snapshot. Pending commands have a 30-second timeout after acceptance. BotCommandCoordinator retains terminal events arriving before HTTP acceptance. Events missed during disconnection still require reconciliation. Remove uses one-based track numbers; move uses zero-based indexes. Moving a track updates the local list optimistically; an HTTP failure requests a fresh server snapshot.

Creating a playlist asks for a name and forwards the displayed queue snapshot through the controller, use case, repository, and remote data source to a single bulk API method. The request carries the playlist name and ordered track URLs, including repeats. Invalid track URLs are rejected before submission. The mobile API method intentionally throws NullPointerException until the backend endpoint is implemented; it sends no HTTP request and creates no playlist. The existing UI reports the failure and releases the command lock. Track-by-track persistence belongs to the future backend implementation, not a mobile request loop.

Queue and CurrentTrack share command locking, pending-command tracking, search, and queue-operation helpers through `core/presentation`. Feature-specific command handlers map their own events to these shared helpers; superseded feature-local helper implementations have been removed. Queue search cancels obsolete requests and resets results when the query, provider or result kind changes.

### Queue validation

Build the Android app to check compilation and packaging:

```powershell
.\gradlew.bat :androidApp:assembleDebug
```

If incremental compilation fails with the TrackSearchPrefix / DirtyData.dirtyLookupSymbols compiler error, retry without incremental compilation for that invocation:

```powershell
.\gradlew.bat :androidApp:assembleDebug '-Pkotlin.incremental=false'
```

Presentation tests include implemented behavior checks and explicit `NotImplementedError` scaffolds. New test cases remain scaffolds unless implementation is explicitly requested. Scaffolds and successful application builds do not establish behavior coverage.

* [/iosApp](./iosApp/iosApp) contains an iOS application. Even if you’re sharing your UI with Compose Multiplatform,
  you need this entry point for your iOS app. This is also where you should add SwiftUI code for your project.

* [/shared](./shared/src) is for code that will be shared across your Compose Multiplatform applications.
  It contains several subfolders:
  - [commonMain](./shared/src/commonMain/kotlin) is for code that’s common for all targets.
  - Other folders are for Kotlin code that will be compiled for only the platform indicated in the folder name.
    For example, if you want to use Apple’s CoreCrypto for the iOS part of your Kotlin app,
    the [iosMain](./shared/src/iosMain/kotlin) folder would be the right place for such calls.
    Similarly, if you want to edit the Desktop (JVM) specific part, the [jvmMain](./shared/src/jvmMain/kotlin)
    folder is the appropriate location.

### Running the apps

Use the run configurations provided by the run widget in your IDE's toolbar. You can also use these commands and options:

- Android app: `./gradlew :androidApp:assembleDebug`
- iOS app: open the [/iosApp](./iosApp) directory in Xcode and run it from there.

### Running tests

Use the run button in your IDE's editor gutter, or run tests using Gradle tasks:

- Android tests: `./gradlew :shared:testAndroidHostTest`
- iOS tests: `./gradlew :shared:iosSimulatorArm64Test`

---

Learn more about [Kotlin Multiplatform](https://www.jetbrains.com/help/kotlin-multiplatform-dev/get-started.html)…

## Biometric application lock

An optional device-local application lock protects the foreground UI before session
restoration and after at least 30 seconds in the background. The profile switch uses
the existing event/effect path and is persisted separately from server profile settings.
Previewable controls receive immutable state rather than platform services.
This is a UI lock, not biometric key-bound token storage.

See [the shared lock controller](core/presentation/src/commonMain/kotlin/com/dc/melodiasmario/core/presentation/biometric/AppLockController.kt) for lock-state coordination and [the biometric UI adapters](core/common-ui/src/commonMain/kotlin/com/dc/melodiasmario/core/commonui/biometric) for platform integration and previews.
## Playlist track feature

The playlisttrack feature displays saved playlist details and supports search/manual track addition, confirmed removal, loading into the playback queue, refresh, and back navigation. Queue and playlist details both use the shared MTrackItemRow component for track display, removal, and reorder accessibility actions. Playlist removal retains its confirmation dialog. The row has a separate reorderEnabled flag so unsupported ordering stays disabled without disabling removal. The screen uses shared search, artwork, and reorder components. Shuffle and movement remain disabled until backend playlist-ordering endpoints are available.

PlaylistTrackViewModel follows the Queue event/handler and naming conventions, calls core runAction/runBotCommand resource runners directly without a playlist-specific Flow wrapper and shared manual-input validation, and retains the shared command coordinator and search controller. Unsupported ordering actions remain disabled.

PlaylistTrackRoute follows the QueueRoute convention: collect UI state, load data through events, and handle navigation/toast effects. PlaylistTrackEffectMessages owns localized toast mapping. PlaylistTrackScreen owns top-bar and floating-action-button configuration and forwards UI actions through onEvent.

## Playback and realtime lifecycle

Realtime subscriptions connect through MobileRealtimeClient.observeGuild. Playback notifications are started by the current-track screen and stopped by the service STOP action or task-removal callback. Artwork loading lives in PlaybackForegroundService. ReorderDragContainer owns its drop-target calculation. Queue commands use the existing realtime command tracking; no separate test-only polling use case is registered.

## Shared bot command completion

QueueViewModel, CurrentTrackViewModel, and PlaylistTrackViewModel use the same core:presentation BotCommandCoordinator. It matches command, guild, and optional user IDs; retains early terminal events; and returns success, failure, or timeout while preserving cancellation. The shared handleCompletion extension dispatches terminal outcomes; feature ViewModels supply refresh and feedback callbacks without duplicating trackPendingCommand methods. There is no playlist-specific coordinator or separate pending-command timer implementation.

## Playlist Covers

Playlist cards, detail headers, and track rows consume the API `artworkUri`. Covers prefer the server-configured custom URL, then the first available track artwork. `MPlaylistArtwork` uses bundled Night Drive, Chill, or Workout covers when a matching name has no usable remote image. Other names use the existing gradient/music fallback. This also covers empty playlists.

The playlist card menu offers **Playlist cover**. Enter an absolute HTTP/HTTPS image URL, or save an empty field to restore automatic artwork. The authenticated request uses `PATCH /playlists/{playlistId}/artwork`; after saving, the playlist list reloads. English and Hungarian labels are included. This flow stores image URLs. Name matching selects local fallback themes; audio-genre inference, manual theme selection, image upload, and AI image generation are not implemented.

### Cover Name Matching

`PlaylistArtworkTheme.fromName` matches complete words, ignoring case and punctuation. Workout/Gym/Fitness/Training and Edzés choose Workout; Chill/Relax/Lofi and Nyugi choose Chill; Night/Drive/Synthwave and Éjszaka choose Night Drive. Ambiguous names use Workout, then Chill, then Night Drive priority. Matching is a display heuristic, not a claim about track genres. The images are bundled with the app and need no image-generation service.

## Source conventions

Kotlin sources use named arguments wherever supported, named function-type parameters, explicit imports, and descriptive dependency names. Public contracts use KDoc and explicit types. Shared UI models are kept in their own named files; preview helpers are private. Domain use cases propagate coroutine cancellation instead of converting it into a resource error. Existing implemented tests are preserved, while empty test cases explicitly throw `NotImplementedError()`.

The project `.editorconfig` enforces Kotlin indentation, final newlines, whitespace cleanup, and disabled trailing commas in IntelliJ-compatible editors.

Playlist cover updates and playlist-track deletion validate HTTP response status, so forbidden responses are surfaced as failures instead of successful mutations.
