package com.dc.melodiasmario.core.model.playbackmonitor

import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack

/**
 * Represents current playback monitor state shared between application layers.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property currentTrack Current track.
 * @property isConnected Is connected.
 * @property isRefreshing Is refreshing.
 * @property lastUpdatedAtMillis Last updated at millis.
 * @property errorMessage Error message.
 */
data class CurrentPlaybackMonitorState(
    val guildId: String? = null,
    val currentTrack: CurrentTrack? = null,
    val isConnected: Boolean = false,
    val isRefreshing: Boolean = false,
    val lastUpdatedAtMillis: Long = 0L,
    val errorMessage: String? = null
)
