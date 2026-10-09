package com.dc.melodiasmario.core.domain.playbackmonitor

import com.dc.melodiasmario.core.model.playbackmonitor.CurrentPlaybackMonitorState
import kotlinx.coroutines.flow.StateFlow

interface CurrentPlaybackMonitorRepository {
    val state: StateFlow<CurrentPlaybackMonitorState>

    fun start(guildId: String)
    fun stop()
    fun refresh()
}