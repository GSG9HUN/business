package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode

/**
 * Defines current track playback presentation state shared with UI consumers.
 *
 * @property currentTrack Current track.
 * @property isPlaying Is playing.
 * @property repeatMode Repeat mode.
 * @property isConnected Whether the playback observation transport is currently connected.
 */
data class CurrentTrackPlaybackUiState(
    val currentTrack: CurrentTrack? = null,
    val isPlaying: Boolean = false,
    val repeatMode: RepeatMode = RepeatMode.NONE,
    val isConnected: Boolean = false
)
