package com.dc.melodiasmario.core.network.currenttrack.dto

import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import kotlinx.serialization.Serializable

/**
 * Represents playback status dto in the API transport contract.
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
@Serializable
data class PlaybackStatusDto(
    val guildId: String,
    val currentTrack: PlaybackTrackDto? = null,
    val isPlaying: Boolean = false,
    val isPaused: Boolean = false,
    val positionSeconds: Int = 0,
    val queueTrackCount: Int = 0,
    val isRepeating: Boolean = false,
    val isRepeatingList: Boolean = false,
    val updatedAtUtc: String = ""
) {

    /**
     * Maps this value to domain.
     */
    fun toDomain(): PlaybackStatus = PlaybackStatus(
        guildId = guildId,
        currentTrack = currentTrack?.toDomain(),
        isPlaying = isPlaying,
        isPaused = isPaused,
        positionSeconds = positionSeconds,
        queueTrackCount = queueTrackCount,
        isRepeating = isRepeating,
        isRepeatingList = isRepeatingList,
        updatedAtUtc = updatedAtUtc
    )
}
