package com.dc.melodiasmario.core.presentation.playbackmonitor

import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.launch

class PlaybackMonitorObserver(
    private val scope: CoroutineScope,
    private val observePlayback: () -> Flow<CurrentPlaybackMonitorState>,
    private val onPlayback: (CurrentPlaybackMonitorState) -> Unit
) {
    private var observationJob: Job? = null

    fun start() {
        if (observationJob?.isActive == true) {
            return
        }

        observationJob = scope.launch {
            observePlayback().collect { playback ->
                onPlayback(playback)
            }
        }
    }

    fun stop() {
        observationJob?.cancel()
        observationJob = null
    }
}