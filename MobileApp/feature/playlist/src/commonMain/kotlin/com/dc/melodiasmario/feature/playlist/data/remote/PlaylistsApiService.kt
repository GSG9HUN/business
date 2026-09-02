package com.dc.melodiasmario.feature.playlist.data.remote

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.feature.playlist.data.remote.dto.PlaylistDto
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.delete
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.patch
import io.ktor.client.request.post
import io.ktor.client.request.setBody

@Single
class PlaylistsApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient
) {
    suspend fun getPlaylists(accessToken: String, guildId: String): List<PlaylistDto> {
        return client.get("$baseUrl/guilds/${guildId}/playlists") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun renamePlaylists(
        accessToken: String,
        playlistId: String,
        newName: String
    ) {
        return client.patch("$baseUrl/playlists/${playlistId}/rename") {
            header("Authorization", "Bearer $accessToken")
            setBody(mapOf("newName" to newName))
        }.body<Unit>()
    }

    suspend fun deletePlaylist(accessToken: String, playlistId: String) {
        return client.delete("$baseUrl/playlists/$playlistId") {
            header("Authorization", "Bearer $accessToken")
        }.body<Unit>()
    }

    suspend fun createPlaylist(accessToken: String, guildId: String, playlistName: String) {
        return client.post("$baseUrl/guilds/$guildId/playlists") {
            header("Authorization", "Bearer $accessToken")
            setBody(mapOf("playlistName" to playlistName))
        }.body<Unit>()
    }
}