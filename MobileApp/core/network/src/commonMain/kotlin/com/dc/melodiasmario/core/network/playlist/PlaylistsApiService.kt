package com.dc.melodiasmario.core.network.playlist

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.botcontrol.dto.BotControlCommandDto
import com.dc.melodiasmario.core.network.playlist.dto.AddPlaylistTrackRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.CreatePlaylistFromTracksRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.CreatePlaylistRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.PlaylistDto
import com.dc.melodiasmario.core.network.playlist.dto.RenamePlaylistRequestDto
import com.dc.melodiasmario.core.network.playlist.dto.SetPlaylistArtworkRequestDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.plugins.expectSuccess
import io.ktor.client.request.delete
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.patch
import io.ktor.client.request.post
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.HttpHeaders
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends playlists API requests through the configured HTTP client.
 */
@Single
class PlaylistsApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the playlists belonging to the requested guild.
     */
    suspend fun getPlaylists(accessToken: String, guildId: String): List<PlaylistDto> {
        return httpClient.get(urlString = "$baseUrl/guilds/${guildId}/playlists") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Sets the playlist cover URL, or restores automatic artwork when the value is absent.
     */
    suspend fun setArtwork(accessToken: String, playlistId: String, artworkUri: String?): Unit {
        httpClient.patch(urlString = "$baseUrl/playlists/$playlistId/artwork") {
            expectSuccess = true
            header(key = HttpHeaders.Authorization, value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = SetPlaylistArtworkRequestDto(artworkUri = artworkUri))
        }
    }

    /**
     * Changes the name of the identified playlist.
     */
    suspend fun renamePlaylist(
        accessToken: String,
        playlistId: String,
        newName: String
    ): Unit {
        return httpClient.patch(urlString = "$baseUrl/playlists/${playlistId}/rename") {
            header(key = "Authorization", value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = RenamePlaylistRequestDto(newName = newName))
        }
            .body<Unit>()
    }

    /**
     * Deletes the identified saved playlist.
     */
    suspend fun deletePlaylist(accessToken: String, playlistId: String): Unit {
        return httpClient.delete(urlString = "$baseUrl/playlists/$playlistId") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body<Unit>()
    }

    /**
     * Creates a named playlist for the requested guild.
     */
    suspend fun createPlaylist(accessToken: String, guildId: String, playlistName: String): PlaylistDto {
        return httpClient.post(urlString = "$baseUrl/guilds/$guildId/playlists") {
            header(key = "Authorization", value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = CreatePlaylistRequestDto(playlistName = playlistName))
        }
            .body()
    }

    /**
     * Requests adding the supplied music query or URL to a saved playlist.
     */
    suspend fun addTrack(accessToken: String, playlistId: String, songUrl: String): BotControlCommandDto {
        return httpClient.post(urlString = "$baseUrl/playlists/$playlistId/tracks") {
            header(key = "Authorization", value = "Bearer $accessToken")
            header(key = HttpHeaders.ContentType, value = ContentType.Application.Json.toString())
            setBody(body = AddPlaylistTrackRequestDto(songUrl = songUrl))
        }
            .body()
    }

    /**
     * Placeholder for creating a playlist and saving its ordered track list in one request.
     * No HTTP request is sent until the backend endpoint and response contract are available.
     */
    @Suppress("UNUSED_PARAMETER")
    suspend fun createPlaylistFromTracks(
        accessToken: String,
        guildId: String,
        request: CreatePlaylistFromTracksRequestDto
    ): PlaylistDto {
        // TODO: Replace this intentional failure with the bulk playlist endpoint call.
        throw NullPointerException("The bulk playlist API endpoint is not implemented yet.")
    }

}
