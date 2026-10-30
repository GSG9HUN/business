package com.dc.melodiasmario.feature.playlist.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time playlists presentation outcomes.
 */
sealed interface PlaylistsEffect : MviEffect {
    /**
     * Describes the artwork saved variant of [PlaylistsEffect].
     */
    data object ArtworkSaved : PlaylistsEffect

    /**
     * Describes the artwork save failed variant of [PlaylistsEffect].
     */
    data object ArtworkSaveFailed : PlaylistsEffect

    /**
     * Describes the navigate to guild selector variant of [PlaylistsEffect].
     */
    data object NavigateToGuildSelector : PlaylistsEffect

    /**
     * Describes the navigate to playlist tracks variant of [PlaylistsEffect].
     *
     * @property playlistId Identifier of the saved playlist.
     */
    data class NavigateToPlaylistTracks(val playlistId: String) : PlaylistsEffect

    /**
     * Describes the playlist created variant of [PlaylistsEffect].
     */
    data object PlaylistCreated : PlaylistsEffect

    /**
     * Describes the playlist create failed variant of [PlaylistsEffect].
     */
    data object PlaylistCreateFailed : PlaylistsEffect

    /**
     * Describes the playlist renamed variant of [PlaylistsEffect].
     */
    data object PlaylistRenamed : PlaylistsEffect

    /**
     * Describes the playlist rename failed variant of [PlaylistsEffect].
     */
    data object PlaylistRenameFailed : PlaylistsEffect

    /**
     * Describes the playlist deleted variant of [PlaylistsEffect].
     */
    data object PlaylistDeleted : PlaylistsEffect

    /**
     * Describes the playlist delete failed variant of [PlaylistsEffect].
     */
    data object PlaylistDeleteFailed : PlaylistsEffect
}
