package com.dc.melodiasmario.core.commonui.guild

/**
 * Provides the bot status text operation for guild bot status text.
 */
fun botStatusText(
    isOnline: Boolean?,
    connectedVoiceChannelName: String?,
    connectedVoiceUserCount: Int,
    isOnlineText: String,
    isOfflineText: String,
    unknownText: String,
    connectedInVoiceSuffix: String
): String {
    if (isOnline == null) {
        return unknownText
    }

    if (!isOnline) {
        return isOfflineText
    }

    val parts = mutableListOf(isOnlineText)

    if (connectedVoiceUserCount > 0) {
        parts += "$connectedVoiceUserCount $connectedInVoiceSuffix"
        return parts.joinToString(separator = " - ")
    }

    connectedVoiceChannelName?.takeIf { textValue -> textValue.isNotBlank() }
        ?.let(block = parts::add)

    return parts.joinToString(separator = " - ")
}
