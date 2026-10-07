package com.dc.melodiasmario.core.data.realtime

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.network.realtime.MobileRealtimeClient
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.currentCoroutineContext
import kotlinx.coroutines.delay
import kotlinx.coroutines.ensureActive
import kotlinx.coroutines.isActive
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single(binds = [RealtimeRepository::class])
class RealtimeRepositoryImpl(
    private val authorizedSessionProvider: AuthorizedSessionProvider,
    private val mobileRealtimeClient: MobileRealtimeClient,
) : RealtimeRepository {
    override val connectionState = mobileRealtimeClient.connectionState

    override fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent> = flow {
        var retryAttempt = 0

        while (currentCoroutineContext().isActive) {
            try {
                val accessToken = authorizedSessionProvider.getValidSession()

                mobileRealtimeClient
                    .observeGuild(accessToken, guildId)
                    .collect { event ->
                        retryAttempt = 0
                        emit(event)
                    }
            } catch (error: CancellationException) {
                throw error
            } catch (_: Throwable) {
                currentCoroutineContext().ensureActive()
            }

            delay(reconnectDelayMillis(retryAttempt))
            retryAttempt++
        }
    }

    override suspend fun disconnect() {
        mobileRealtimeClient.disconnect()
    }

    private fun reconnectDelayMillis(retryAttempt: Int): Long {
        val multiplier = 1 shl retryAttempt.coerceAtMost(MaxReconnectBackoffStep)
        return InitialReconnectDelayMillis * multiplier
    }

    private companion object {
        const val InitialReconnectDelayMillis = 1_000L
        const val MaxReconnectBackoffStep = 4
    }
}
