package com.dc.melodiasmario.core.network.playlisttrack

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.botcontrol.dto.BotControlCommandDto
import com.dc.melodiasmario.core.network.playlisttrack.dto.PlaylistDetailDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.plugins.expectSuccess
import io.ktor.client.request.delete
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.post
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends playlist track API requests through the configured HTTP client.
 */
@Single
class PlaylistTrackApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the saved playlist details and ordered tracks for the requested playlist.
     */
    suspend fun getPlaylist(accessToken: String, playlistId: String): PlaylistDetailDto {
        return httpClient.get(urlString = "$baseUrl/playlists/$playlistId") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Removes the track identified by its playlist order number.
     */
    suspend fun removeTrack(accessToken: String, playlistId: String, trackNumber: Int): Unit {
        return httpClient.delete(urlString = "$baseUrl/playlists/$playlistId/tracks/$trackNumber") {
            expectSuccess = true
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Requests loading the saved playlist into the playback queue.
     */
    suspend fun loadPlaylist(accessToken: String, playlistId: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/playlists/$playlistId/load") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }
}
