package com.dc.melodiasmario.core.presentation

import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.launch

/**
 * Provides bot command realtime observer for the presentation layer.
 */
class BotCommandRealtimeObserver(
    private val coroutineScope: CoroutineScope,
    private val observeGuildRealtime: (guildId: String) -> Flow<RealtimeEvent>,
    private val onSucceeded: suspend (botControlCommandRealtimeEvent: BotControlCommandRealtimeEvent) -> Unit,
    private val onFailed: suspend (botControlCommandRealtimeEvent: BotControlCommandRealtimeEvent) -> Unit
) {
    private var job: Job? = null
    private var currentGuildId: String? = null

    /**
     * Starts bot command realtime observer for the supplied configuration.
     */
    fun start(guildId: String): Unit {
        if (currentGuildId == guildId && job?.isActive == true) {
            return
        }

        currentGuildId = guildId
        job?.cancel()
        job = coroutineScope.launch {
            observeGuildRealtime(guildId).collect { event ->

                if (event.guildId == guildId) {
                    handle(event = event)
                }
            }
        }
    }

    /**
     * Clears the state and resources owned by BotCommandRealtimeObserver.
     */
    fun clear(): Unit {
        job?.cancel()
        job = null
        currentGuildId = null
    }

    private suspend fun handle(event: RealtimeEvent) {
        when (event.eventName) {
            MobileRealtimeEventNames.BotControlCommandSucceeded -> {
                val commandEvent = event as? BotControlCommandRealtimeEvent ?: return
                onSucceeded(commandEvent)
            }

            MobileRealtimeEventNames.BotControlCommandFailed,
            MobileRealtimeEventNames.BotControlCommandInterrupted -> {
                val commandEvent = event as? BotControlCommandRealtimeEvent ?: return
                onFailed(commandEvent)
            }
        }
    }
}
