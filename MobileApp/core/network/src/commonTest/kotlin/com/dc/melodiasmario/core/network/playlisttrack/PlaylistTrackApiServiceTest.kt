package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.network.client.configureMelodiasHttpClient
import io.ktor.client.HttpClient
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.respond
import io.ktor.client.plugins.ClientRequestException
import io.ktor.http.*
import kotlinx.coroutines.test.runTest
import kotlin.test.*

class PlaylistTrackApiServiceTest {
    @Test
    fun detailUsesAuthorizedEndpointAndMapsBackendTrackOrder() = runTest {
        val engine = MockEngine { request ->
            assertEquals("/api/playlists/17", request.url.encodedPath)
            assertEquals("Bearer token", request.headers[HttpHeaders.Authorization])
            respond(
                """{"id":"17","guildId":"guild","name":"Late Night","songCount":2,"duration":20,
                  "tracks":[
                    {"orderNumber":2,"title":"Second","author":"Artist","duration":10,"trackUri":"https://example.com/2"},
                    {"orderNumber":1,"title":"First","author":"Artist","duration":10,"trackUri":"https://example.com/1"}]}""",
                HttpStatusCode.OK,
                headersOf(HttpHeaders.ContentType, ContentType.Application.Json.toString()),
            )
        }
        val client = HttpClient(engine) { configureMelodiasHttpClient() }
        try {
            val detail = PlaylistTrackApiService("https://example.com/api", client).getPlaylist("token", "17").toDomain()
            assertEquals(listOf("First", "Second"), detail.tracks.map { it.title })
            assertEquals("https://example.com/1", detail.tracks.first().trackUri)
        } finally {
            client.close()
        }
    }

    @Test
    fun deleteUsesBackendOneBasedTrackNumber() = runTest {
        val client = HttpClient(MockEngine { request ->
            assertEquals(HttpMethod.Delete, request.method)
            assertEquals("/api/playlists/17/tracks/3", request.url.encodedPath)
            respond("", HttpStatusCode.NoContent)
        }) { configureMelodiasHttpClient() }
        try {
            PlaylistTrackApiService("https://example.com/api", client).removeTrack("token", "17", 3)
        } finally {
            client.close()
        }
    }

    @Test
    fun forbiddenDeletionIsNotTreatedAsSuccess() = runTest {
        val client = HttpClient(MockEngine {
            respond("Forbidden", HttpStatusCode.Forbidden)
        }) { configureMelodiasHttpClient() }
        try {
            assertFailsWith<ClientRequestException> {
                PlaylistTrackApiService("https://example.com/api", client).removeTrack("token", "17", 1)
            }
        } finally {
            client.close()
        }
    }
}
