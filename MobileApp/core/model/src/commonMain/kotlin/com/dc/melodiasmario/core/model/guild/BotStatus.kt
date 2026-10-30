package com.dc.melodiasmario.core.model.guild

/**
 * Represents bot status shared between application layers.
 *
 * @property isOnline Is online.
 * @property connectedVoiceChannelName Connected voice channel name.
 * @property connectedVoiceUserCount Connected voice user count.
 */
data class BotStatus(
    val isOnline: Boolean,
    val connectedVoiceChannelName: String? = null,
    val connectedVoiceUserCount: Int = 0
)
