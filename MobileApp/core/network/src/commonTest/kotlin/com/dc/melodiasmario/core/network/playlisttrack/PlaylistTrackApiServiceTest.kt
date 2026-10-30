package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.network.client.configureMelodiasHttpClient
import io.ktor.client.HttpClient
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.respond
import io.ktor.client.plugins.ClientRequestException
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import io.ktor.http.HttpMethod
import io.ktor.http.HttpStatusCode
import io.ktor.http.encodedPath
import io.ktor.http.headers
import io.ktor.http.headersOf
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlinx.coroutines.test.runTest

class PlaylistTrackApiServiceTest {
    @Test
    fun detailUsesAuthorizedEndpointAndMapsBackendTrackOrder() = runTest {
        val engine = MockEngine { request ->
            assertEquals(expected = "/api/playlists/17", actual = request.url.encodedPath)
            assertEquals(expected = "Bearer token", actual = request.headers[HttpHeaders.Authorization])
            respond(
                content = """{"id":"17","guildId":"guild","name":"Late Night","songCount":2,"duration":20,
                  "tracks":[
                    {"orderNumber":2,"title":"Second","author":"Artist","duration":10,"trackUri":"https://example.com/2"},
                    {"orderNumber":1,"title":"First","author":"Artist","duration":10,"trackUri":"https://example.com/1"}]}""",
                status = HttpStatusCode.OK,
                headers = headersOf(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            )
        }
        val httpClient = HttpClient(engine = engine) { configureMelodiasHttpClient() }
        try {
            val playlistTrackApiService = PlaylistTrackApiService(
                baseUrl = "https://example.com/api",
                httpClient = httpClient
            ).getPlaylist(
                accessToken = "token",
                playlistId = "17"
            )
                .toDomain()
            assertEquals(expected = listOf("First", "Second"), actual = playlistTrackApiService.tracks.map { track -> track.title })
            assertEquals(expected = "https://example.com/1", actual = playlistTrackApiService.tracks.first().trackUri)
        } finally {
            httpClient.close()
        }
    }

    @Test
    fun deleteUsesBackendOneBasedTrackNumber() = runTest {
        val httpClient = HttpClient(
            engine = MockEngine { request ->
                assertEquals(expected = HttpMethod.Delete, actual = request.method)
                assertEquals(expected = "/api/playlists/17/tracks/3", actual = request.url.encodedPath)
                respond(content = "", status = HttpStatusCode.NoContent)
            }
        ) { configureMelodiasHttpClient() }
        try {
            PlaylistTrackApiService(
                baseUrl = "https://example.com/api",
                httpClient = httpClient
            ).removeTrack(
                accessToken = "token",
                playlistId = "17",
                trackNumber = 3
            )
        } finally {
            httpClient.close()
        }
    }

    @Test
    fun forbiddenDeletionIsNotTreatedAsSuccess() = runTest {
        val httpClient = HttpClient(
            engine = MockEngine {
                respond(content = "Forbidden", status = HttpStatusCode.Forbidden)
            }
        ) { configureMelodiasHttpClient() }
        try {
            assertFailsWith<ClientRequestException> {
                PlaylistTrackApiService(
                    baseUrl = "https://example.com/api",
                    httpClient = httpClient
                ).removeTrack(
                    accessToken = "token",
                    playlistId = "17",
                    trackNumber = 1
                )
            }
        } finally {
            httpClient.close()
        }
    }
}
