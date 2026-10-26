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

@Single
class QueuePlaylistSaveManager(
    private val createPlaylistFromQueueUseCase: CreatePlaylistFromQueueUseCase,
    private val feedback: QueuePlaylistSaveFeedback,
) {
    private val scope = CoroutineScope(SupervisorJob() + Dispatchers.Default)
    private val mutex = Mutex()
    private val savingGuildIds = mutableSetOf<String>()

    suspend fun save(
        guildId: String,
        name: String,
        trackUrls: List<String>,
    ): QueuePlaylistSaveResult {
        val snapshot = trackUrls.toList()
        val playlistName = name.trim()

        val operation = mutex.withLock {
            check(savingGuildIds.add(guildId)) {
                "A playlist save is already in progress for this guild."
            }

            scope.async {
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

                    feedback.show(result)
                    result
                } finally {
                    withContext(NonCancellable) {
                        mutex.withLock {
                            savingGuildIds.remove(guildId)
                        }
                    }
                }
            }
        }

        return operation.await()
    }
}