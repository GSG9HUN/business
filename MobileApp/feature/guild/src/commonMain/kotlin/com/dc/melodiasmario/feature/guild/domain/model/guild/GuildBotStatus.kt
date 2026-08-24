package com.dc.melodiasmario.feature.guild.domain.model.guild

data class BotStatus(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int = 0,
)
