package com.dc.melodiasmario.core.domain.playlist.usecase

import com.dc.melodiasmario.core.domain.playlist.PlaylistsRepository
import com.dc.melodiasmario.core.model.playlist.Playlist
import org.koin.core.annotation.Factory

/** Submits the displayed queue snapshot as one ordered track list. */
@Factory
class CreatePlaylistFromQueueUseCase(
    private val playlistsRepository: PlaylistsRepository
) {
    /**
     * Executes the create playlist from queue domain operation.
     */
    suspend operator fun invoke(
        guildId: String,
        name: String,
        trackUrls: List<String>
    ): Playlist {
        require(value = name.isNotBlank() && trackUrls.isNotEmpty())
        require(
            value = trackUrls.all { trackUrl -> trackUrl.startsWith(
                prefix = "https://"
            ) || trackUrl.startsWith(
                prefix = "http://"
            ) }
        ) {
            "Some queue tracks have no usable URL."
        }

        return playlistsRepository.createPlaylistFromTracks(
            guildId = guildId,
            playlistName = name.trim(),
            trackUrls = trackUrls
        )
    }
}
