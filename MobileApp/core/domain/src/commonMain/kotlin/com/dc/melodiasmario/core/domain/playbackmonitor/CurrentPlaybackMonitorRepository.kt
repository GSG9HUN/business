package com.dc.melodiasmario.core.domain.playbackmonitor

import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import kotlinx.coroutines.flow.StateFlow

/**
 * Defines current playback monitor data operations independently of their concrete data sources.
 */
interface CurrentPlaybackMonitorRepository {
    /**
     * Read-only playback snapshots and connection state observed by playback consumers.
     */
    val playbackMonitorState: StateFlow<CurrentPlaybackMonitorState>

    /**
     * Starts current playback monitor repository for the supplied configuration.
     */
    fun start(guildId: String): Unit

    /**
     * Requests a fresh playback-monitor snapshot.
     */
    fun refresh(): Unit
}
