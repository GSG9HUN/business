package com.dc.melodiasmario.feature.playlist.presentation

/**
 * Defines the interactions handled by playlists presentation logic.
 */
sealed interface PlaylistsEvent {
    /**
     * Describes the artwork clicked variant of [PlaylistsEvent].
     *
     * @property playlistId Identifier of the saved playlist.
     */
    data class ArtworkClicked(val playlistId: String) : PlaylistsEvent

    /**
     * Describes the artwork draft changed variant of [PlaylistsEvent].
     *
     * @property artworkUri Optional artwork URI returned by the API.
     */
    data class ArtworkDraftChanged(val artworkUri: String) : PlaylistsEvent

    /**
     * Describes the artwork confirmed variant of [PlaylistsEvent].
     */
    data object ArtworkConfirmed : PlaylistsEvent

    /**
     * Describes the avatar clicked variant of [PlaylistsEvent].
     */
    data object AvatarClicked : PlaylistsEvent

    /**
     * Describes the refresh clicked variant of [PlaylistsEvent].
     */
    data object RefreshClicked : PlaylistsEvent

    /**
     * Describes the search clicked variant of [PlaylistsEvent].
     */
    data object SearchClicked : PlaylistsEvent

    /**
     * Describes the add playlist clicked variant of [PlaylistsEvent].
     */
    data object AddPlaylistClicked : PlaylistsEvent

    /**
     * Describes the dialog dismissed variant of [PlaylistsEvent].
     */
    data object DialogDismissed : PlaylistsEvent

    /**
     * Describes the create playlist confirmed variant of [PlaylistsEvent].
     */
    data object CreatePlaylistConfirmed : PlaylistsEvent

    /**
     * Describes the search confirmed variant of [PlaylistsEvent].
     */
    data object SearchConfirmed : PlaylistsEvent

    /**
     * Describes the rename playlist confirmed variant of [PlaylistsEvent].
     */
    data object RenamePlaylistConfirmed : PlaylistsEvent

    /**
     * Describes the delete playlist confirmed variant of [PlaylistsEvent].
     */
    data object DeletePlaylistConfirmed : PlaylistsEvent

    /**
     * Describes the load playlists variant of [PlaylistsEvent].
     *
     * @property guildId Identifier of the guild owning this value.
     */
    data class LoadPlaylists(val guildId: String) : PlaylistsEvent

    /**
     * Describes the playlist clicked variant of [PlaylistsEvent].
     *
     * @property playlistId Identifier of the saved playlist.
     */
    data class PlaylistClicked(val playlistId: String) : PlaylistsEvent

    /**
     * Describes the search draft changed variant of [PlaylistsEvent].
     *
     * @property query Query.
     */
    data class SearchDraftChanged(val query: String) : PlaylistsEvent

    /**
     * Describes the playlist name draft changed variant of [PlaylistsEvent].
     *
     * @property playlistName Playlist name.
     */
    data class PlaylistNameDraftChanged(val playlistName: String) : PlaylistsEvent

    /**
     * Describes the rename playlist clicked variant of [PlaylistsEvent].
     *
     * @property playlistId Identifier of the saved playlist.
     * @property playlistName Playlist name.
     */
    data class RenamePlaylistClicked(val playlistId: String, val playlistName: String) : PlaylistsEvent

    /**
     * Describes the delete playlist clicked variant of [PlaylistsEvent].
     *
     * @property playlistId Identifier of the saved playlist.
     * @property playlistName Playlist name.
     */
    data class DeletePlaylistClicked(val playlistId: String, val playlistName: String) : PlaylistsEvent
}
