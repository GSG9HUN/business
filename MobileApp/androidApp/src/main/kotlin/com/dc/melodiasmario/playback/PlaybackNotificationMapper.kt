package com.dc.melodiasmario.playback

import android.app.Notification
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.graphics.Bitmap
import android.graphics.Color
import android.support.v4.media.session.MediaSessionCompat
import androidx.core.app.NotificationCompat
import androidx.media.app.NotificationCompat.MediaStyle
import com.dc.melodiasmario.R
import com.dc.melodiasmario.core.commonui.playbacknotification.PlaybackNotificationAction

object PlaybackNotificationMapper {
    fun create(
        context: Context,
        mediaSessionToken: MediaSessionCompat.Token,
        guildId: String,
        title: String,
        artist: String,
        artwork: Bitmap?,
        isPlaying: Boolean,
    ): Notification {
        PlaybackNotificationChannel.ensureCreated(context)

        return NotificationCompat.Builder(context, PlaybackNotificationChannel.ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setLargeIcon(artwork)
            .setContentTitle(title)
            .setContentText(artist)
            .setOnlyAlertOnce(true)
            .setOngoing(false)
            .setAutoCancel(true)
            .setDeleteIntent(pendingIntent(context, PlaybackNotificationAction.STOP, guildId))
            .setCategory(NotificationCompat.CATEGORY_TRANSPORT)
            .setVisibility(NotificationCompat.VISIBILITY_PUBLIC)
            .setColor(NOTIFICATION_COLOR)
            .setColorized(true)
            .setStyle(
                MediaStyle()
                    .setMediaSession(mediaSessionToken)
                    .setShowActionsInCompactView(0, 1, 2)
            )
            .addAction(action(context, PlaybackNotificationAction.PREVIOUS, R.drawable.ic_previous, "Previous", guildId))
            .addAction(
                action(
                    context,
                    PlaybackNotificationAction.PLAY_PAUSE,
                    if (isPlaying) R.drawable.ic_pause else R.drawable.ic_play,
                    if (isPlaying) "Pause" else "Play",
                    guildId,
                )
            )
            .addAction(action(context, PlaybackNotificationAction.NEXT, R.drawable.ic_next, "Next", guildId))
            .build()
    }

    private fun action(
        context: Context,
        action: String,
        icon: Int,
        title: String,
        guildId: String,
    ): NotificationCompat.Action {
        return NotificationCompat.Action(icon, title, pendingIntent(context, action, guildId))
    }

    private fun pendingIntent(context: Context, action: String, guildId: String): PendingIntent {
        val intent = Intent(context, PlaybackForegroundService::class.java)
            .setAction(action)
            .putExtra(PlaybackNotificationAction.EXTRA_GUILD_ID, guildId)

        return PendingIntent.getService(
            context,
            action.hashCode(),
            intent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE,
        )
    }

    private val NOTIFICATION_COLOR = Color.rgb(35, 39, 47)
}
