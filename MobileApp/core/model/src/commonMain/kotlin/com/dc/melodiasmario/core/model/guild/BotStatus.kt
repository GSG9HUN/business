package com.dc.melodiasmario.core.model.guild

data class BotStatus(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int = 0,
)
