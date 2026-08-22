package com.dc.melodiasmario.feature.guild.data.remote.dto

import com.dc.melodiasmario.feature.guild.domain.model.BotStatus
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