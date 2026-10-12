package com.dc.melodiasmario.core.commonui.playbacknotification

import android.content.Context
import android.content.Intent
import androidx.core.content.ContextCompat

actual class PlaybackNotificationController(
    private val context: Context,
) {
    actual fun start(guildId: String) {
        val intent = Intent()
            .setClassName(context.packageName, PLAYBACK_SERVICE_CLASS)
            .setAction(PlaybackNotificationAction.START)
            .putExtra(PlaybackNotificationAction.EXTRA_GUILD_ID, guildId)

        ContextCompat.startForegroundService(context, intent)
    }

    actual fun stop() {
        val intent = Intent()
            .setClassName(context.packageName, PLAYBACK_SERVICE_CLASS)
            .setAction(PlaybackNotificationAction.STOP)

        context.startService(intent)
    }

    private companion object {
        const val PLAYBACK_SERVICE_CLASS =
            "com.dc.melodiasmario.playback.PlaybackForegroundService"
    }
}
