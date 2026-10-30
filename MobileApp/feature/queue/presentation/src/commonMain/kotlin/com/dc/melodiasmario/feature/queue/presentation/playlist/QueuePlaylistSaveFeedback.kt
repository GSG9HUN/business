package com.dc.melodiasmario.feature.queue.presentation.playlist

/**
 * Provides queue playlist save feedback for the playlist layer.
 */
interface QueuePlaylistSaveFeedback {
    /**
     * Displays the supplied playlist-save feedback.
     */
    suspend fun show(result: QueuePlaylistSaveResult): Unit
}
