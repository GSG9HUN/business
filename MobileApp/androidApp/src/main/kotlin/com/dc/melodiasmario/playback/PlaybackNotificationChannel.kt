package com.dc.melodiasmario.playback

import android.app.NotificationChannel
import android.app.NotificationManager
import android.content.Context
import android.os.Build

object PlaybackNotificationChannel {
    const val ID = "playback"

    fun ensureCreated(context: Context) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return

        val manager = context.getSystemService(NotificationManager::class.java)
        val channel = NotificationChannel(
            ID,
            "Playback",
            NotificationManager.IMPORTANCE_LOW,
        )

        manager.createNotificationChannel(channel)
    }
}