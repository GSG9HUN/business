package com.dc.melodiasmario.core.model.currenttrack

import com.dc.melodiasmario.core.model.guild.BotStatus

/**
 * Represents current track shared between application layers.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property guildName Guild name.
 * @property guildIconUrl Guild icon url.
 * @property guildBotStatus Guild bot status.
 * @property currentTrack Current track.
 * @property queuedTracks Queued tracks.
 * @property isPlaying Is playing.
 * @property isPaused Is paused.
 * @property positionSeconds Playback position in seconds.
 * @property queueTrackCount Queue track count.
 * @property isRepeating Is repeating.
 * @property isRepeatingList Is repeating list.
 * @property repeatMode Repeat mode.
 * @property updatedAtUtc Updated at utc.
 */
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
    val updatedAtUtc: String = ""
)
