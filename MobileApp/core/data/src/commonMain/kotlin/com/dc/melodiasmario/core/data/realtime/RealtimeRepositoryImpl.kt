package com.dc.melodiasmario.core.data.realtime

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.realtime.RealtimeRepository
import com.dc.melodiasmario.core.model.realtime.RealtimeConnectionState
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import com.dc.melodiasmario.core.network.realtime.MobileRealtimeClient
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.ExperimentalCoroutinesApi
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.currentCoroutineContext
import kotlinx.coroutines.delay
import kotlinx.coroutines.ensureActive
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.emitAll
import kotlinx.coroutines.flow.emptyFlow
import kotlinx.coroutines.flow.filter
import kotlinx.coroutines.flow.flatMapLatest
import kotlinx.coroutines.flow.flow
import kotlinx.coroutines.flow.shareIn
import kotlinx.coroutines.isActive
import org.koin.core.annotation.Single

/**
 * Implements realtime data access using the injected data sources.
 */
@OptIn(ExperimentalCoroutinesApi::class)
@Single(binds = [RealtimeRepository::class])
class RealtimeRepositoryImpl(
    private val authorizedSessionProvider: AuthorizedSessionProvider,
    private val mobileRealtimeClient: MobileRealtimeClient
) : RealtimeRepository {
    override val connectionState: StateFlow<RealtimeConnectionState> = mobileRealtimeClient.connectionState

    private val coroutineScope = CoroutineScope(context = SupervisorJob() + Dispatchers.Default)
    private val selectedGuildId = MutableStateFlow<String?>(value = null)

    private val sharedEvents = selectedGuildId.flatMapLatest { guildId ->

        if (guildId == null) {
            emptyFlow()
        } else {
            observeWithReconnect(guildId = guildId)
        }
    }
        .shareIn(
            scope = coroutineScope,
            started = SharingStarted.WhileSubscribed(stopTimeoutMillis = 0),
            replay = 0
        )

    override fun observeGuildRealtime(guildId: String): Flow<RealtimeEvent> = flow {
        selectedGuildId.value = guildId
        emitAll(sharedEvents.filter { sharedEvent -> sharedEvent.guildId == guildId })
    }

    override suspend fun disconnect(): Unit {
        selectedGuildId.value = null
        mobileRealtimeClient.disconnect()
    }

    private fun observeWithReconnect(guildId: String): Flow<RealtimeEvent> = flow {
        var retryAttempt = 0

        while (currentCoroutineContext().isActive) {
            try {
                val accessToken = authorizedSessionProvider.getValidSession()

                mobileRealtimeClient.observeGuild(accessToken = accessToken, guildId = guildId)
                    .collect { event ->
                        retryAttempt = 0
                        emit(event)
                    }
            } catch (error: CancellationException) {
                throw error
            } catch (_: Throwable) {
                currentCoroutineContext().ensureActive()
            }

            delay(duration = reconnectDelayMillis(retryAttempt = retryAttempt).milliseconds)
            retryAttempt++
        }
    }

    private fun reconnectDelayMillis(retryAttempt: Int): Long {
        val multiplier = 1 shl retryAttempt.coerceAtMost(maximumValue = MAX_RECONNECT_BACKOFF_STEP)
        return INITIAL_RECONNECT_DELAY_MILLIS * multiplier
    }

    private companion object {
        const val INITIAL_RECONNECT_DELAY_MILLIS = 1_000L
        const val MAX_RECONNECT_BACKOFF_STEP = 4
    }
}
