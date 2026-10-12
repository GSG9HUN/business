package com.dc.melodiasmario.feature.currenttrack.presentation.command

import com.dc.melodiasmario.core.domain.realtime.usecase.ObserveGuildRealtimeUseCase
import com.dc.melodiasmario.core.model.realtime.BotControlCommandRealtimeEvent
import com.dc.melodiasmario.core.model.realtime.MobileRealtimeEventNames
import com.dc.melodiasmario.core.model.realtime.RealtimeEvent
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.launch

internal class CurrentTrackCommandRealtimeObserver(
    private val scope: CoroutineScope,
    private val observeGuildRealtimeUseCase: ObserveGuildRealtimeUseCase,
    private val onSucceeded: suspend (BotControlCommandRealtimeEvent) -> Unit,
    private val onFailed: suspend (BotControlCommandRealtimeEvent) -> Unit,
) {
    private var job: Job? = null

    fun start(guildId: String) {
        job?.cancel()
        job = scope.launch {
            observeGuildRealtimeUseCase(guildId).collect { event ->
                handle(event)
            }
        }
    }

    fun clear() {
        job?.cancel()
        job = null
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