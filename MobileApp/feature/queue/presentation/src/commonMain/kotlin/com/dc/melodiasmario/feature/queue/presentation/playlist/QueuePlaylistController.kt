package com.dc.melodiasmario.feature.queue.presentation.playlist

import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import com.dc.melodiasmario.feature.queue.presentation.QueueUiState
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow

internal class QueuePlaylistController(
    private val saveManager: QueuePlaylistSaveManager,
    private val state: MutableStateFlow<QueueUiState>,
    private val effects: MutableSharedFlow<QueueEffect>,
    private val currentGuildIdProvider: () -> String?,
    private val onCloseDialog: () -> Unit,
) {
    suspend fun createPlaylist() {
        val guildId = currentGuildIdProvider() ?: return
        val snapshot = state.value
        val name = snapshot.playlistNameDraft.trim()

        if (name.isBlank() || snapshot.tracks.isEmpty()) {
            return
        }

        val trackUrls = snapshot.tracks.map { it.id }

        try {
            val result = saveManager.save(
                guildId = guildId,
                name = name,
                trackUrls = trackUrls,
            )

            if (result.succeeded && currentGuildIdProvider() == guildId) {
                onCloseDialog()
            }
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            effects.emit(
                QueueEffect.PlaylistSaveFailed(
                    detail = error.message,
                )
            )
        }
    }
}