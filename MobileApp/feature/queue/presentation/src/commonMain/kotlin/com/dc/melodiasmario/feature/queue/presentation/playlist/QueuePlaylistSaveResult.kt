package com.dc.melodiasmario.feature.queue.presentation.playlist

data class QueuePlaylistSaveResult(
    val playlistName: String,
    val succeeded: Boolean,
    val errorDetail: String? = null
)
