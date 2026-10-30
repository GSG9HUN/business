package com.dc.melodiasmario.core.presentation.playbackmonitor

import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.launch

/**
 * Provides playback monitor observer for the playbackmonitor layer.
 */
class PlaybackMonitorObserver(
    private val coroutineScope: CoroutineScope,
    private val observePlayback: () -> Flow<CurrentPlaybackMonitorState>,
    private val onPlayback: (playbackMonitorState: CurrentPlaybackMonitorState) -> Unit
) {
    private var observationJob: Job? = null

    /**
     * Starts playback monitor observer for the supplied configuration.
     */
    fun start(): Unit {
        if (observationJob?.isActive == true) {
            return
        }

        observationJob = coroutineScope.launch {
            observePlayback().collect { playback ->
                onPlayback(playback)
            }
        }
    }

    /**
     * Stops the active playback monitor observer.
     */
    fun stop(): Unit {
        observationJob?.cancel()
        observationJob = null
    }
}
