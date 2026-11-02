package com.dc.melodiasmario.feature.playlist.presentation

/**
 * Provides playlists dialog for the presentation layer.
 */
sealed interface PlaylistsDialog {
    /**
     * Describes the none variant of [PlaylistsDialog].
     */
    data object None : PlaylistsDialog

    /**
     * Describes the create variant of [PlaylistsDialog].
     */
    data object Create : PlaylistsDialog

    /**
     * Describes the search variant of [PlaylistsDialog].
     */
    data object Search : PlaylistsDialog

    /**
     * Describes the artwork variant of [PlaylistsDialog].
     *
     * @property playlistId Identifier of the saved playlist.
     */
    data class Artwork(val playlistId: String) : PlaylistsDialog

    /**
     * Describes the rename variant of [PlaylistsDialog].
     *
     * @property playlistId Identifier of the saved playlist.
     * @property playlistName Playlist name.
     */
    data class Rename(
        val playlistId: String,
        val playlistName: String
    ) : PlaylistsDialog

    /**
     * Describes the delete variant of [PlaylistsDialog].
     *
     * @property playlistId Identifier of the saved playlist.
     * @property playlistName Playlist name.
     */
    data class Delete(
        val playlistId: String,
        val playlistName: String
    ) : PlaylistsDialog
}
