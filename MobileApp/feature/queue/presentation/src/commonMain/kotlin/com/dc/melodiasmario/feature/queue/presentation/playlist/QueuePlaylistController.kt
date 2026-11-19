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

        val captured = snapshot.displayTracks.filter { track -> track.queueState == "Played" || track.queueState == "Skipped" } +
            snapshot.displayTracks.filter { track -> track.queueState == "Playing" } + snapshot.tracks

        if (name.isBlank() || captured.isEmpty()) {
            return
        }

        try {
            require(value = captured.all { track -> !track.queueItemId.isNullOrBlank() }) {
                "Queue occurrence IDs are missing. Refresh the queue before saving."
            }
            // Deduplicate overlapping snapshot sections by occurrence, never by source URL.
            val ordered = captured.distinctBy { track -> track.queueItemId }
            val result = queuePlaylistSaveManager.save(
                guildId = guildId,
                name = name,
                trackUrls = ordered.map { track -> track.id },
                queueItemIds = ordered.map { track -> requireNotNull(value = track.queueItemId) }
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
