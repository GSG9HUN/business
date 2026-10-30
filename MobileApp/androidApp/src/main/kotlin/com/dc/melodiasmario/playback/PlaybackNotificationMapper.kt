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

/**
 * Groups playback notification mapper values used by the application.
 */
object PlaybackNotificationMapper {
    /**
     * Creates the playback notification from the supplied media metadata.
     */
    fun create(
        context: Context,
        mediaSessionToken: MediaSessionCompat.Token,
        guildId: String,
        title: String,
        artist: String,
        artwork: Bitmap?,
        isPlaying: Boolean
    ): Notification {
        PlaybackNotificationChannel.ensureCreated(context = context)

        return NotificationCompat.Builder(context, PlaybackNotificationChannel.ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setLargeIcon(artwork)
            .setContentTitle(title)
            .setContentText(artist)
            .setOnlyAlertOnce(true)
            .setOngoing(false)
            .setAutoCancel(true)
            .setDeleteIntent(pendingIntent(context = context, action = PlaybackNotificationAction.STOP, guildId = guildId))
            .setCategory(NotificationCompat.CATEGORY_TRANSPORT)
            .setVisibility(NotificationCompat.VISIBILITY_PUBLIC)
            .setColor(NOTIFICATION_COLOR)
            .setColorized(true)
            .setStyle(
                MediaStyle()
                    .setMediaSession(mediaSessionToken)
                    .setShowActionsInCompactView(0, 1, 2)
            )
            .addAction(
                action(
                    context = context,
                    action = PlaybackNotificationAction.PREVIOUS,
                    icon = R.drawable.ic_previous,
                    title = "Previous",
                    guildId = guildId
                )
            )
            .addAction(
                action(
                    context = context,
                    action = PlaybackNotificationAction.PLAY_PAUSE,
                    icon = if (isPlaying) R.drawable.ic_pause else R.drawable.ic_play,
                    title = if (isPlaying) "Pause" else "Play",
                    guildId = guildId
                )
            )
            .addAction(
                action(
                    context = context,
                    action = PlaybackNotificationAction.NEXT,
                    icon = R.drawable.ic_next,
                    title = "Next",
                    guildId = guildId
                )
            )
            .build()
    }

    private fun action(
        context: Context,
        action: String,
        icon: Int,
        title: String,
        guildId: String
    ): NotificationCompat.Action {
        return NotificationCompat.Action(icon, title, pendingIntent(context = context, action = action, guildId = guildId))
    }

    private fun pendingIntent(context: Context, action: String, guildId: String): PendingIntent {
        val intent = Intent(context, PlaybackForegroundService::class.java)
            .setAction(action)
            .putExtra(PlaybackNotificationAction.EXTRA_GUILD_ID, guildId)

        return PendingIntent.getService(
            context,
            action.hashCode(),
            intent,
            PendingIntent.FLAG_UPDATE_CURRENT or PendingIntent.FLAG_IMMUTABLE
        )
    }

    private val NOTIFICATION_COLOR = Color.rgb(35, 39, 47)
}
