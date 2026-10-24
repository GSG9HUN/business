package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import com.dc.melodiasmario.core.model.playlist.Playlist
import org.koin.core.annotation.Factory

/** Submits the displayed queue snapshot as one ordered track list. */
@Factory
class CreatePlaylistFromQueueUseCase(
    private val repository: PlaylistsRepository,
) {
    suspend operator fun invoke(
        guildId: String,
        name: String,
        trackUrls: List<String>,
    ): Playlist {
        require(name.isNotBlank() && trackUrls.isNotEmpty())
        require(trackUrls.all { it.startsWith("https://") || it.startsWith("http://") }) {
            "Some queue tracks have no usable URL."
        }

        return repository.createPlaylistFromTracks(
            guildId = guildId,
            playlistName = name.trim(),
            trackUrls = trackUrls,
        )
    }
}
