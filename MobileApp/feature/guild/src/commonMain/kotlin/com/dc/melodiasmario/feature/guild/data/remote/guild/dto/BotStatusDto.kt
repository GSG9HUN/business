package com.dc.melodiasmario.feature.guild.data.remote.guild.dto

import com.dc.melodiasmario.feature.guild.domain.model.guild.BotStatus
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