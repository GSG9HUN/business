package com.dc.melodiasmario.core.model.playbackmonitor

import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack

data class CurrentPlaybackMonitorState(
    val guildId: String? = null,
    val currentTrack: CurrentTrack? = null,
    val isConnected: Boolean = false,
    val isRefreshing: Boolean = false,
    val lastUpdatedAtMillis: Long = 0L,
    val errorMessage: String? = null,
)