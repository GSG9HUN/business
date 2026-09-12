package com.dc.melodiasmario.core.network.guild.dto

import com.dc.melodiasmario.core.model.guild.BotStatus
import kotlinx.serialization.Serializable

@Serializable
data class BotStatusDto(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int,
)

fun BotStatusDto.toDomain() = BotStatus(
    isOnline = isOnline,
    connectedVoiceChannelName = connectedVoiceChannelName,
    connectedVoiceUserCount = connectedVoiceUserCount
)