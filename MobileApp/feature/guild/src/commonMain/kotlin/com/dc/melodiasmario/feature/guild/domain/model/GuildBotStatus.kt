package com.dc.melodiasmario.feature.guild.domain.model

data class BotStatus(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int = 0,
)
