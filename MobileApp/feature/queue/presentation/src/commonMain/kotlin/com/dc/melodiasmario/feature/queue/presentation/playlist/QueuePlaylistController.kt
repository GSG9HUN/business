package com.dc.melodiasmario.feature.queue.presentation.playlist

import com.dc.melodiasmario.feature.queue.presentation.QueueEffect
import com.dc.melodiasmario.feature.queue.presentation.QueueUiState
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow

/**
 * Owns queue playlist coordination and its lifecycle-bound operations.
 */
internal class QueuePlaylistController(
    private val queuePlaylistSaveManager: QueuePlaylistSaveManager,
    private val uiState: MutableStateFlow<QueueUiState>,
    private val effects: MutableSharedFlow<QueueEffect>,
    private val currentGuildIdProvider: () -> String?,
    private val onCloseDialog: () -> Unit
) {
    /**
     * Creates a named playlist for the requested guild.
     */
    suspend fun createPlaylist(): Unit {
        val guildId = currentGuildIdProvider() ?: return
        val snapshot = uiState.value
        val name = snapshot.playlistNameDraft.trim()

        if (name.isBlank() || snapshot.tracks.isEmpty()) {
            return
        }

        val trackUrls = snapshot.tracks.map { track -> track.id }

        try {
            val result = queuePlaylistSaveManager.save(
                guildId = guildId,
                name = name,
                trackUrls = trackUrls
            )

            if (result.succeeded && currentGuildIdProvider() == guildId) {
                onCloseDialog()
            }
        } catch (error: CancellationException) {
            throw error
        } catch (error: Exception) {
            effects.emit(
                value = QueueEffect.PlaylistSaveFailed(
                    detail = error.message
                )
            )
        }
    }
}
