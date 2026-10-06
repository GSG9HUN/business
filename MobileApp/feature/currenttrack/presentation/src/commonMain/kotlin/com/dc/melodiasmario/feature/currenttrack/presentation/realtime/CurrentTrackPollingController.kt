package com.dc.melodiasmario.feature.currenttrack.presentation.realtime

import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import kotlin.time.Duration.Companion.milliseconds

internal class CurrentTrackPollingController(
    private val scope: CoroutineScope,
    private val intervalMillis: Long,
    private val onRefresh: suspend () -> Unit,
) {
    private var pollingJob: Job? = null

    fun start() {
        if (pollingJob?.isActive == true) return

        pollingJob = scope.launch {
            while (isActive) {
                delay(intervalMillis.milliseconds)
                onRefresh()
            }
        }
    }

    fun stop() {
        pollingJob?.cancel()
        pollingJob = null
    }

    fun clear() {
        stop()
    }
}
