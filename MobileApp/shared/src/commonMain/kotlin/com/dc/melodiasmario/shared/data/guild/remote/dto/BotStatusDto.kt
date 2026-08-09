package com.dc.melodiasmario.shared.data.guild.remote.dto

import com.dc.melodiasmario.shared.domain.guild.model.BotStatus
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