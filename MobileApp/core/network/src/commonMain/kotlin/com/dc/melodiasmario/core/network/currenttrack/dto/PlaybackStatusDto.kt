package com.dc.melodiasmario.core.network.currenttrack.dto

import com.dc.melodiasmario.core.model.currenttrack.PlaybackStatus
import kotlinx.serialization.Serializable

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
    val updatedAtUtc: String = "",
) {

    fun toDomain() = PlaybackStatus(
        guildId = guildId,
        currentTrack = currentTrack?.toDomain(),
        isPlaying = isPlaying,
        isPaused = isPaused,
        positionSeconds = positionSeconds,
        queueTrackCount = queueTrackCount,
        isRepeating = isRepeating,
        isRepeatingList = isRepeatingList,
        updatedAtUtc = updatedAtUtc,
    )
}
