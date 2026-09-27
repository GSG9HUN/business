package com.dc.melodiasmario.feature.playlist.presentation

sealed interface PlaylistsEvent {
    data object AvatarClicked : PlaylistsEvent
    data object RefreshClicked : PlaylistsEvent
    data object SearchClicked : PlaylistsEvent
    data object AddPlaylistClicked : PlaylistsEvent
    data object DialogDismissed : PlaylistsEvent
    data object CreatePlaylistConfirmed : PlaylistsEvent
    data object SearchConfirmed : PlaylistsEvent
    data object RenamePlaylistConfirmed : PlaylistsEvent
    data object DeletePlaylistConfirmed : PlaylistsEvent
    data class LoadPlaylists(val guildId: String) : PlaylistsEvent
    data class PlaylistClicked(val playlistId: String) : PlaylistsEvent
    data class SearchDraftChanged(val query: String) : PlaylistsEvent
    data class PlaylistNameDraftChanged(val playlistName: String) : PlaylistsEvent
    data class RenamePlaylistClicked(val playlistId: String, val playlistName: String) : PlaylistsEvent
    data class DeletePlaylistClicked(val playlistId: String, val playlistName: String) : PlaylistsEvent
}
