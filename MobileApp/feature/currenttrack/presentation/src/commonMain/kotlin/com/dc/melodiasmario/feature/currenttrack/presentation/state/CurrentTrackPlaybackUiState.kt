package com.dc.melodiasmario.feature.currenttrack.presentation.state

import com.dc.melodiasmario.core.model.currenttrack.CurrentTrack
import com.dc.melodiasmario.core.model.currenttrack.RepeatMode

/**
 * Defines current track playback presentation state shared with UI consumers.
 *
 * @property currentTrack Current track.
 * @property isPlaying Is playing.
 * @property repeatMode Repeat mode.
 */
data class CurrentTrackPlaybackUiState(
    val currentTrack: CurrentTrack? = null,
    val isPlaying: Boolean = false,
    val repeatMode: RepeatMode = RepeatMode.NONE
)
