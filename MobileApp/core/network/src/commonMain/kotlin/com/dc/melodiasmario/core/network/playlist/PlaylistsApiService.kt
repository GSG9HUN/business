package com.dc.melodiasmario.core.network.playlist

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.playlist.dto.CreatePlaylistFromTracksRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.CreatePlaylistRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.PlaylistDto
import com.dc.melodiasmario.core.network.playlist.dto.AddPlaylistTrackRequestDto
import com.dc.melodiasmario.core.network.botcontrol.dto.BotControlCommandDto
import com.dc.melodiasmario.core.network.playlist.dto.RenamePlaylistRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.SetPlaylistArtworkRequestDto
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
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders

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

    suspend fun setArtwork(accessToken: String, playlistId: String, artworkUri: String?) {
        client.patch("$baseUrl/playlists/$playlistId/artwork") {
            header(HttpHeaders.Authorization, "Bearer $accessToken")
            header(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            setBody(SetPlaylistArtworkRequestDto(artworkUri))
        }
    }

    suspend fun renamePlaylist(
        accessToken: String,
        playlistId: String,
        newName: String
    ) {
        return client.patch("$baseUrl/playlists/${playlistId}/rename") {
            header("Authorization", "Bearer $accessToken")
            header(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            setBody(RenamePlaylistRequestDto(newName = newName))
        }.body<Unit>()
    }

    suspend fun deletePlaylist(accessToken: String, playlistId: String) {
        return client.delete("$baseUrl/playlists/$playlistId") {
            header("Authorization", "Bearer $accessToken")
        }.body<Unit>()
    }

    suspend fun createPlaylist(accessToken: String, guildId: String, playlistName: String): PlaylistDto {
        return client.post("$baseUrl/guilds/$guildId/playlists") {
            header("Authorization", "Bearer $accessToken")
            header(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            setBody(CreatePlaylistRequestDto(playlistName = playlistName))
        }.body()
    }

    suspend fun addTrack(accessToken: String, playlistId: String, songUrl: String): BotControlCommandDto {
        return client.post("$baseUrl/playlists/$playlistId/tracks") {
            header("Authorization", "Bearer $accessToken")
            header(HttpHeaders.ContentType, ContentType.Application.Json.toString())
            setBody(AddPlaylistTrackRequestDto(songUrl))
        }.body()
    }
    /**
     * Placeholder for creating a playlist and saving its ordered track list in one request.
     * No HTTP request is sent until the backend endpoint and response contract are available.
     */
    @Suppress("UNUSED_PARAMETER")
    suspend fun createPlaylistFromTracks(
        accessToken: String,
        guildId: String,
        request: CreatePlaylistFromTracksRequestDto,
    ): PlaylistDto {
        // TODO: Replace this intentional failure with the bulk playlist endpoint call.
        throw NullPointerException("The bulk playlist API endpoint is not implemented yet.")
    }

}
