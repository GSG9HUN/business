# network

Kotlin Multiplatform HTTP services, remote data sources, and serialization DTOs
used by the application data layer.

## Overview

- Typed HTTP services and remote data sources for authenticated application operations.
- Serialization DTOs and realtime transport adapters shared through the data layer.
- Platform HTTP clients configured through the existing dependency injection modules.

`commonMain` depends on `core:common` and `core:model` plus Ktor, serialization, coroutines, and Koin. The data layer consumes its remote data sources; network code does not depend on `core:data` or `core:domain`. Gradle configures Android, `iosArm64`, and `iosSimulatorArm64` targets, with platform-specific transport adapters in their own source sets.

## Subfolders

### src/commonMain/

HTTP services, remote data source contracts and implementations, serializable DTOs, DI registration, and realtime contracts.

**Files:**

- [`PlaylistsApiService.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/network/playlist/PlaylistsApiService.kt)
- [`CreatePlaylistFromTracksRequestDto.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/network/playlist/dto/CreatePlaylistFromTracksRequestDto.kt)
- [`MobileRealtimeClient.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/network/realtime/MobileRealtimeClient.kt)
- [`NetworkModule.kt`](src/commonMain/kotlin/com/dc/melodiasmario/core/network/di/NetworkModule.kt)

**Purpose:** HTTP services, remote data source contracts and implementations, serializable DTOs, DI registration, and realtime contracts.

---

### src/androidMain/

Android implementation of the realtime client; Gradle provides OkHttp and SignalR dependencies.

**Files:**

- [`MobileRealtimeClient.android.kt`](src/androidMain/kotlin/com/dc/melodiasmario/core/network/realtime/MobileRealtimeClient.android.kt)

**Purpose:** Android implementation of the realtime client; Gradle provides OkHttp and SignalR dependencies.

---

### src/iosMain/

iOS implementation of the realtime client; Gradle provides the Darwin HTTP engine.

**Files:**

- [`MobileRealtimeClient.ios.kt`](src/iosMain/kotlin/com/dc/melodiasmario/core/network/realtime/MobileRealtimeClient.ios.kt)

**Purpose:** iOS implementation of the realtime client; Gradle provides the Darwin HTTP engine.

---

### src/commonTest/

Shared HTTP adapter tests and test scaffolds using existing Kotlin test, coroutine test, and Ktor mock dependencies.

**Files:**

- [`HttpClientTest.kt`](src/commonTest/kotlin/com/dc/melodiasmario/core/network/client/HttpClientTest.kt)
- [`PlaylistArtworkApiServiceTest.kt`](src/commonTest/kotlin/com/dc/melodiasmario/core/network/playlisttrack/PlaylistArtworkApiServiceTest.kt)

**Purpose:** Shared HTTP adapter tests and test scaffolds using existing Kotlin test, coroutine test, and Ktor mock dependencies.

---

## Components

### PlaylistsApiService

**Purpose:** `PlaylistsApiService.createPlaylistFromTracks` sends one authenticated POST to
`/guilds/{guildId}/playlists/from-queue`. The request contains `playlistName`,
ordered `trackUrls`, and matching `queueItemIds`. Duplicate URLs are retained.

The backend validates guild ownership and every stored occurrence, then creates
metadata and tracks in one transaction with the configured limits. It returns
201 with a `PlaylistDto`-compatible summary. Validation errors return 400;
name conflicts and the guild playlist cap return 409. No individual add-track
requests or external source lookups are performed by this flow.

**Methods:**

- `getPlaylists` retrieves guild playlists with an authenticated GET and no request body.
- `setArtwork` sends a separate PATCH with a nullable artwork URL.
- `createPlaylistFromTracks` submits the captured URLs and occurrence IDs as one request.

**Validation Checks:**

Run the following command from the MobileApp root.

Build the Android app with
`./gradlew.bat :androidApp:assembleDebug '-Pkotlin.incremental=false'`.
This verifies compilation, not successful playlist persistence.

---

### QueueDto

**Purpose:** Staging HTTP client and artwork contracts coexist with the from-queue snapshot endpoint. QueueDto maps waiting and display tracks separately; playlist retrieval has no artwork argument or request body.

---

### createHttpClient and NetworkModule

**Purpose:** The module uses Koin registration and a shared Ktor client configuration with JSON serialization. Authorization headers are sanitized in HTTP logging.

HTTP calls use the declared Ktor parameter names for URLs, headers, and request bodies. Flow emission and parsing calls also use named arguments where supported. Platform SDK callbacks and Java APIs keep their required signatures and positional arguments.

---

## Related Components

- [core:data](../data): authenticated repository implementations.
- [core:domain](../domain): repository contracts and use cases.
- [core:model](../model): application models mapped from network DTOs.
