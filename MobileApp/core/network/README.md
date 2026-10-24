# network

Kotlin Multiplatform HTTP services, remote data sources, and serialization DTOs
used by the application data layer.

## Bulk playlist creation

The mobile queue playlist flow reaches
`PlaylistsApiService.createPlaylistFromTracks` through
`PlaylistsRemoteDataSource`. `CreatePlaylistFromTracksRequestDto` carries the
playlist name and ordered track URLs, including duplicates.

The method intentionally throws `NullPointerException` instead of performing
an HTTP request. The backend route and final response contract are not
implemented by this placeholder. No empty playlist is created and no
individual add-track requests are sent by this flow. The future backend
implementation is responsible for saving the submitted tracks.

The current return type is `PlaylistDto`, consistent with the existing
playlist-creation mapping. Replace the API stub when the backend contract is
available; cancellation and failures continue through the existing layers.

## Related modules

- `core/data`: authenticated repository implementations.
- `core/domain`: repository contracts and use cases.
- `core/model`: application models mapped from network DTOs.

## Validation

Build the Android app with
`./gradlew.bat :androidApp:assembleDebug '-Pkotlin.incremental=false'`.
This verifies compilation, not successful playlist persistence.
