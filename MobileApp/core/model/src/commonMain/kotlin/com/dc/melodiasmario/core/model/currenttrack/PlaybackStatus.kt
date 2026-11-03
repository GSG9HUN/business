package com.dc.melodiasmario.core.model.currenttrack

import com.dc.melodiasmario.core.model.guild.Guild
import com.dc.melodiasmario.core.model.queue.Queue

/**
 * Represents playback status shared between application layers.
 *
 * @property guildId Identifier of the guild owning this value.
 * @property currentTrack Current track.
 * @property isPlaying Is playing.
 * @property isPaused Is paused.
 * @property positionSeconds Playback position in seconds.
 * @property queueTrackCount Queue track count.
 * @property isRepeating Is repeating.
 * @property isRepeatingList Is repeating list.
 * @property updatedAtUtc Updated at utc.
 */
data class PlaybackStatus(
    val guildId: String,
    val currentTrack: Track?,
    val isPlaying: Boolean,
    val isPaused: Boolean,
    val positionSeconds: Int,
    val queueTrackCount: Int,
    val isRepeating: Boolean,
    val isRepeatingList: Boolean,
    val updatedAtUtc: String
) {
    /**
     * Maps this value to current track.
     */
    fun toCurrentTrack(
        queue: Queue,
        guild: Guild?
    ): CurrentTrack = CurrentTrack(
        guildId = guildId,
        guildName = guild?.name.orEmpty(),
        guildIconUrl = guild?.iconUrl,
        guildBotStatus = guild?.botStatus,
        currentTrack = currentTrack,
        queuedTracks = queue.tracks,
        isPlaying = isPlaying,
        isPaused = isPaused,
        positionSeconds = positionSeconds,
        queueTrackCount = queueTrackCount,
        isRepeating = isRepeating,
        isRepeatingList = isRepeatingList,
        repeatMode = toRepeatMode(),
        updatedAtUtc = updatedAtUtc
    )

    private fun toRepeatMode() = when {
        isRepeating -> RepeatMode.ONE
        isRepeatingList -> RepeatMode.ALL
        else -> RepeatMode.NONE
    }
}
