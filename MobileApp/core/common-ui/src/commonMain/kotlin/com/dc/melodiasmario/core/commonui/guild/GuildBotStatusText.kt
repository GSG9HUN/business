package com.dc.melodiasmario.core.commonui.guild

fun botStatusText(
    isOnline: Boolean?,
    connectedVoiceChannelName: String?,
    connectedVoiceUserCount: Int,
    isOnlineText: String,
    isOfflineText: String,
    unknownText: String,
    connectedInVoiceSuffix: String,
): String {
    if (isOnline == null) return unknownText

    if (!isOnline) return isOfflineText

    val parts = mutableListOf(isOnlineText)

    if (connectedVoiceUserCount > 0) {
        parts += "$connectedVoiceUserCount $connectedInVoiceSuffix"
        return parts.joinToString(" - ")
    }

    connectedVoiceChannelName?.takeIf { it.isNotBlank() }?.let(parts::add)

    return parts.joinToString(" - ")
}
