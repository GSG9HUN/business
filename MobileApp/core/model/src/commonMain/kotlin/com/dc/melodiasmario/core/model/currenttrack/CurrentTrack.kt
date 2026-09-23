package com.dc.melodiasmario.core.model.currenttrack

import com.dc.melodiasmario.core.model.guild.BotStatus

data class CurrentTrack(
    val guildId: String = "",
    val guildName: String = "",
    val guildIconUrl: String? = null,
    val guildBotStatus: BotStatus? = null,
    val currentTrack: Track? = null,
    val queuedTracks: List<Track> = emptyList(),
    val isPlaying: Boolean = false,
    val isPaused: Boolean = false,
    val positionSeconds: Int = 0,
    val queueTrackCount: Int = 0,
    val isRepeating: Boolean = false,
    val isRepeatingList: Boolean = false,
    val repeatMode: RepeatMode = RepeatMode.NONE,
    val updatedAtUtc: String = "",
)
