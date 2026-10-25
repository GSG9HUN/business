package com.dc.melodiasmario.core.presentation

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.launch
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent

class BotCommandRealtimeObserver(
    private val scope: CoroutineScope,
    private val observeGuildRealtime: (String) -> Flow<RealtimeEvent>,
    private val onSucceeded: suspend (BotControlCommandRealtimeEvent) -> Unit,
    private val onFailed: suspend (BotControlCommandRealtimeEvent) -> Unit,
) {
    private var job: Job? = null
    private var currentGuildId: String? = null


    fun start(guildId: String) {
        if (currentGuildId == guildId && job?.isActive == true) {
            return
        }
        currentGuildId = guildId
        job?.cancel()
        job = scope.launch {
            observeGuildRealtime(guildId).collect { event ->
                if (event.guildId == guildId) {
                    handle(event)
                }
            }
        }
    }

    fun clear() {
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