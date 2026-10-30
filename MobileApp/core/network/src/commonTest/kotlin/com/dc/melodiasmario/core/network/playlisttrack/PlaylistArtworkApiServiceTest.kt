package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.network.client.configureMelodiasHttpClient
import com.dc.melodiasmario.core.network.playlist.PlaylistsApiService
import com.dc.melodiasmario.core.network.playlist.dto.PlaylistDto
import com.dc.melodiasmario.core.network.playlisttrack.dto.PlaylistDetailDto
import io.ktor.client.HttpClient
import io.ktor.client.plugins.ClientRequestException
import io.ktor.client.engine.mock.MockEngine
import io.ktor.client.engine.mock.respond
import io.ktor.http.HttpHeaders
import io.ktor.http.HttpMethod
import io.ktor.http.HttpStatusCode
import io.ktor.http.content.TextContent
import kotlinx.coroutines.test.runTest
import kotlinx.serialization.json.Json
import kotlinx.serialization.json.JsonNull
import kotlinx.serialization.json.JsonPrimitive
import kotlinx.serialization.json.jsonObject
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertFailsWith

class PlaylistArtworkApiServiceTest {
    @Test
    fun forbiddenCoverUpdateIsNotReportedAsSuccess() = runTest {
        val client = HttpClient(MockEngine { respond("Forbidden", HttpStatusCode.Forbidden) }) {
            configureMelodiasHttpClient()
        }
        try {
            assertFailsWith<ClientRequestException> {
                PlaylistsApiService("https://example.com/api", client).setArtwork("token", "17", null)
            }
        } finally {
            client.close()
        }
    }

    @Test
    fun settingAndClearingCoverSendAuthorizedPatchWithExplicitNull() = runTest {
        val bodies = mutableListOf<String>()
        val client = HttpClient(MockEngine { request ->
            assertEquals(HttpMethod.Patch, request.method)
            assertEquals("/api/playlists/17/artwork", request.url.encodedPath)
            assertEquals("Bearer token", request.headers[HttpHeaders.Authorization])
            bodies += (request.body as TextContent).text
            respond("", HttpStatusCode.NoContent)
        }) { configureMelodiasHttpClient() }
        try {
            val service = PlaylistsApiService("https://example.com/api", client)
            service.setArtwork("token", "17", "https://example.com/cover.jpg")
            service.setArtwork("token", "17", null)
            assertEquals(JsonPrimitive("https://example.com/cover.jpg"),
                Json.parseToJsonElement(bodies[0]).jsonObject["artworkUri"])
            assertEquals(JsonNull, Json.parseToJsonElement(bodies[1]).jsonObject["artworkUri"])
        } finally {
            client.close()
        }
    }

    @Test
    fun detailAndSummaryKeepArtworkWhileOlderResponsesRemainCompatible() {
        val summary = Json.decodeFromString<PlaylistDto>(
            """{"id":"17","name":"Mix","songCount":1,"duration":120,"artworkUri":"https://example.com/custom.jpg"}"""
        ).toDomain()
        assertEquals("https://example.com/custom.jpg", summary.artworkUri)
        val detail = Json.decodeFromString<PlaylistDetailDto>(
            """{"id":"17","guildId":"7","name":"Mix","songCount":1,"duration":120,
                "artworkUri":"https://example.com/custom.jpg",
                "tracks":[{"orderNumber":1,"title":"Song","author":"Artist","duration":120,
                    "trackUri":"https://example.com/song","artworkUri":"https://example.com/track.jpg"}]}"""
        ).toDomain()
        assertEquals(summary.artworkUri, detail.artworkUri)
        assertEquals("https://example.com/track.jpg", detail.tracks.single().artworkUri)
        assertNull(Json.decodeFromString<PlaylistDto>(
            """{"id":"17","name":"Mix","songCount":0,"duration":0}"""
        ).toDomain().artworkUri)
    }
}
