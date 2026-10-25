# MobileApp

This is a Kotlin Multiplatform project targeting Android and iOS.

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

Queue mutations use command-id tracking and terminal realtime events. A matching terminal event or a terminal state returned in the HTTP response releases the command lock and refreshes the server snapshot. Pending commands have a 30-second timeout after acceptance. Terminal events missed before HTTP acceptance or during disconnection still require reconciliation. Remove uses one-based track numbers; move uses zero-based indexes. Moving a track updates the local list optimistically; an HTTP failure requests a fresh server snapshot.

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

Presentation tests are currently ignored scaffolds that throw `NotImplementedError`; they do not verify command completion or search behavior. A successful build does not establish behavior coverage.

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
