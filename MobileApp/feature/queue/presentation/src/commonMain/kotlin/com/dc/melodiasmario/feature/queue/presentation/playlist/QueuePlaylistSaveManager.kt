package com.dc.melodiasmario.feature.queue.presentation.playlist

import com.dc.melodiasmario.core.domain.playlist.usecase.CreatePlaylistFromQueueUseCase
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.NonCancellable
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.async
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlinx.coroutines.withContext
import org.koin.core.annotation.Single

/**
 * Provides queue playlist save manager for the playlist layer.
 */
@Single
class QueuePlaylistSaveManager(
    private val createPlaylistFromQueueUseCase: CreatePlaylistFromQueueUseCase,
    private val feedback: QueuePlaylistSaveFeedback
) {
    private val coroutineScope = CoroutineScope(context = SupervisorJob() + Dispatchers.Default)
    private val mutex = Mutex()
    private val savingGuildIds = mutableSetOf<String>()

    /**
     * Stores the supplied value in queue playlist save manager.
     */
    suspend fun save(
        guildId: String,
        name: String,
        trackUrls: List<String>
    ): QueuePlaylistSaveResult {
        val snapshot = trackUrls.toList()
        val playlistName = name.trim()

        val operation = mutex.withLock {
            check(value = savingGuildIds.add(element = guildId)) {
                "A playlist save is already in progress for this guild."
            }

            coroutineScope.async {
                try {
                    val result = try {
                        val playlist = createPlaylistFromQueueUseCase(
                            guildId = guildId,
                            name = playlistName,
                            trackUrls = snapshot
                        )

                        QueuePlaylistSaveResult(
                            playlistName = playlist.name,
                            succeeded = true
                        )
                    } catch (error: CancellationException) {
                        throw error
                    } catch (error: Exception) {
                        QueuePlaylistSaveResult(
                            playlistName = playlistName,
                            succeeded = false,
                            errorDetail = error.message
                        )
                    }

                    feedback.show(result = result)
                    result
                } finally {
                    withContext(context = NonCancellable) {
                        mutex.withLock {
                            savingGuildIds.remove(element = guildId)
                        }
                    }
                }
            }
        }

        return operation.await()
    }
}
