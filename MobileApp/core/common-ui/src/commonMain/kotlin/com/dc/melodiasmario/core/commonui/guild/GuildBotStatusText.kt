package com.dc.melodiasmario.core.commonui.guild

import com.dc.melodiasmario.core.model.guild.BotStatus

fun botStatusText(
    botStatus: BotStatus?,
    isOnlineText: String,
    isOfflineText: String,
    unknownText: String,
    connectedInVoiceSuffix: String,
): String {
    val status = botStatus ?: return unknownText

    if (!status.isOnline) return isOfflineText

    val parts = mutableListOf(isOnlineText)

    if (status.connectedVoiceUserCount > 0) {
        parts += "${status.connectedVoiceUserCount} $connectedInVoiceSuffix"
        return parts.joinToString(" - ")
    }

    status.connectedVoiceChannelName?.takeIf { it.isNotBlank() }?.let(parts::add)

    return parts.joinToString(" - ")
}
