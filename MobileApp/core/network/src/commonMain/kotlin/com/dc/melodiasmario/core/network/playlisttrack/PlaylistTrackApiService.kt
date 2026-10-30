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

@Single
class PlaylistTrackApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient
) {
    suspend fun getPlaylist(accessToken: String, playlistId: String): PlaylistDetailDto {
        return client.get("$baseUrl/playlists/$playlistId") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun removeTrack(accessToken: String, playlistId: String, trackNumber: Int) {
        return client.delete("$baseUrl/playlists/$playlistId/tracks/$trackNumber") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun loadPlaylist(accessToken: String, playlistId: String): BotControlCommandDto {
        return client.post("$baseUrl/playlists/$playlistId/load") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }
}
