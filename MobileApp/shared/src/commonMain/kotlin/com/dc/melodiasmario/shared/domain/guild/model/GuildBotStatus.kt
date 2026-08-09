package com.dc.melodiasmario.shared.domain.guild.model

data class BotStatus(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int = 0,
)
