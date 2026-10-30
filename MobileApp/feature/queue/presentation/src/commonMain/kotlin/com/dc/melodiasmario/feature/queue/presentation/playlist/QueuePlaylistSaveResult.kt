package com.dc.melodiasmario.feature.queue.presentation.playlist

/**
 * Provides queue playlist save result for the playlist layer.
 *
 * @property playlistName Playlist name.
 * @property succeeded Succeeded.
 * @property errorDetail Error detail.
 */
data class QueuePlaylistSaveResult(
    val playlistName: String,
    val succeeded: Boolean,
    val errorDetail: String? = null
)
