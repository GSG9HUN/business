package com.dc.melodiasmario.core.network.guild.dto

import com.dc.melodiasmario.core.model.guild.BotStatus
import kotlinx.serialization.Serializable

/**
 * Represents bot status dto in the API transport contract.
 *
 * @property isOnline Is online.
 * @property connectedVoiceChannelName Connected voice channel name.
 * @property connectedVoiceUserCount Connected voice user count.
 */
@Serializable
data class BotStatusDto(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): BotStatus = BotStatus(
        isOnline = isOnline,
        connectedVoiceChannelName = connectedVoiceChannelName,
        connectedVoiceUserCount = connectedVoiceUserCount
    )
}
