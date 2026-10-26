package com.dc.melodiasmario.feature.queue.presentation.playlist

interface QueuePlaylistSaveFeedback {
    suspend fun show(result: QueuePlaylistSaveResult)
}