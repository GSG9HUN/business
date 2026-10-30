package com.dc.melodiasmario.playback

import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import android.os.Build

/**
 * Groups playback notification channel values used by the application.
 */
object PlaybackNotificationChannel {
    /**
     * Id exposed by playback notification channel.
     */
    const val ID: String = "playback"

    /**
     * Creates the playback notification channel when required.
     */
    fun ensureCreated(context: Context): Unit {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return
        }

        val manager = context.getSystemService(NotificationManager::class.java)
        val channel = NotificationChannel(
            ID,
            "Playback",
            NotificationManager.IMPORTANCE_LOW
        )

        manager.createNotificationChannel(channel)
    }
}
