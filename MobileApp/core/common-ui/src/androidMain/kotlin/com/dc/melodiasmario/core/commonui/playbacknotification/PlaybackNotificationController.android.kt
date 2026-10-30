package com.dc.melodiasmario.core.commonui.playbacknotification

import android.content.Context
import android.content.Intent
import androidx.core.content.ContextCompat

/**
 * Owns playback notification coordination and its lifecycle-bound operations.
 */
actual class PlaybackNotificationController(
    private val context: Context
) {
    /**
     * Starts playback notification controller for the supplied configuration.
     */
    actual fun start(guildId: String): Unit {
        val intent = Intent()
            .setClassName(context.packageName, PLAYBACK_SERVICE_CLASS)
            .setAction(PlaybackNotificationAction.START)
            .putExtra(PlaybackNotificationAction.EXTRA_GUILD_ID, guildId)

        ContextCompat.startForegroundService(context, intent)
    }

    private companion object {
        const val PLAYBACK_SERVICE_CLASS =
            "com.dc.melodiasmario.playback.PlaybackForegroundService"
    }
}
