package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.network.client.configureMelodiasHttpClient
import com.dc.melodiasmario.core.network.playlist.PlaylistsApiService
import com.dc.melodiasmario.core.network.playlist.dto.PlaylistDto
import com.dc.melodiasmario.core.network.playlisttrack.dto.PlaylistDetailDto
import io.ktor.client.HttpClient
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.respond
import io.ktor.client.plugins.ClientRequestException
import io.ktor.http.HttpHeaders
import io.ktor.http.HttpMethod
import io.ktor.http.HttpStatusCode
import io.ktor.http.content.TextContent
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNull
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonNull
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.jsonObject

class PlaylistArtworkApiServiceTest {
    @Test
    fun forbiddenCoverUpdateIsNotReportedAsSuccess() = runTest {
        val httpClient = HttpClient(engine = MockEngine { respond(content = "Forbidden", status = HttpStatusCode.Forbidden) }) {
            configureMelodiasHttpClient()
        }
        try {
            assertFailsWith<ClientRequestException> {
                PlaylistsApiService(
                    baseUrl = "https://example.com/api",
                    httpClient = httpClient
                ).setArtwork(
                    accessToken = "token",
                    playlistId = "17",
                    artworkUri = null
                )
            }
        } finally {
            httpClient.close()
        }
    }

    @Test
    fun settingAndClearingCoverSendAuthorizedPatchWithExplicitNull() = runTest {
        val bodies = mutableListOf<String>()
        val httpClient = HttpClient(
            engine = MockEngine { request ->
                assertEquals(expected = HttpMethod.Patch, actual = request.method)
                assertEquals(expected = "/api/playlists/17/artwork", actual = request.url.encodedPath)
                assertEquals(expected = "Bearer token", actual = request.headers[HttpHeaders.Authorization])
                bodies += (request.body as TextContent).text
                respond(content = "", status = HttpStatusCode.NoContent)
            }
        ) { configureMelodiasHttpClient() }
        try {
            val playlistsApiService = PlaylistsApiService(baseUrl = "https://example.com/api", httpClient = httpClient)
            playlistsApiService.setArtwork(accessToken = "token", playlistId = "17", artworkUri = "https://example.com/cover.jpg")
            playlistsApiService.setArtwork(accessToken = "token", playlistId = "17", artworkUri = null)
            assertEquals(
                expected = JsonPrimitive(value = "https://example.com/cover.jpg"),
                actual = Json.parseToJsonElement(bodies[0]).jsonObject["artworkUri"]
            )
            assertEquals(expected = JsonNull, actual = Json.parseToJsonElement(bodies[1]).jsonObject["artworkUri"])
        } finally {
            httpClient.close()
        }
    }

    @Test
    fun detailAndSummaryKeepArtworkWhileOlderResponsesRemainCompatible(): Unit {
        val summary = Json.decodeFromString<PlaylistDto>(
            """{"id":"17","name":"Mix","songCount":1,"duration":120,"artworkUri":"https://example.com/custom.jpg"}"""
        )
            .toDomain()
        assertEquals(expected = "https://example.com/custom.jpg", actual = summary.artworkUri)
        val detail = Json.decodeFromString<PlaylistDetailDto>(
            """{"id":"17","guildId":"7","name":"Mix","songCount":1,"duration":120,
                "artworkUri":"https://example.com/custom.jpg",
                "tracks":[{"orderNumber":1,"title":"Song","author":"Artist","duration":120,
                    "trackUri":"https://example.com/song","artworkUri":"https://example.com/track.jpg"}]}"""
        )
            .toDomain()
        assertEquals(expected = summary.artworkUri, actual = detail.artworkUri)
        assertEquals(expected = "https://example.com/track.jpg", actual = detail.tracks.single().artworkUri)
        assertNull(actual = Json.decodeFromString<PlaylistDto>(
            """{"id":"17","name":"Mix","songCount":0,"duration":0}"""
        )
            .toDomain().artworkUri)
    }
}
