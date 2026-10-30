package com.dc.melodiasmario.feedback

import android.Manifest
import android.annotation.SuppressLint
import android.app.NotificationChannel
import android.app.NotificationManager
import android.app.PendingIntent
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.os.Build
import androidx.core.app.NotificationCompat
import androidx.core.app.NotificationManagerCompat
import androidx.core.content.ContextCompat
import com.dc.melodiasmario.R
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistSaveFeedback
import com.dc.melodiasmario.feature.queue.presentation.playlist.QueuePlaylistSaveResult
import java.util.UUID
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext

/**
 * Provides android playlist save feedback for the feedback layer.
 */
class AndroidPlaylistSaveFeedback(
    private val context: Context,
    private val appVisibility: AppVisibility,
    private val toastHostState: MToastHostState
) : QueuePlaylistSaveFeedback {

    override suspend fun show(result: QueuePlaylistSaveResult): Unit {
        withContext(context = Dispatchers.Main.immediate) {
            val message = context.getString(
                if (result.succeeded) {
                    R.string.playlist_save_success
                } else {
                    R.string.playlist_save_failure
                },
                result.playlistName
            )

            if (appVisibility.isForeground) {
                toastHostState.showToast(
                    toast = MToastData(
                        message = if (result.succeeded) {
                            message
                        } else {
                            listOfNotNull(
                                message,
                                result.errorDetail?.takeIf { textValue ->
                                    textValue.isNotBlank()
                                }
                            ).joinToString(separator = "\n")
                        },
                        type = if (result.succeeded) {
                            MToastType.Success
                        } else {
                            MToastType.Error
                        }
                    )
                )
            } else {
                showNotification(result = result, message = message)
            }
        }
    }

    @SuppressLint("MissingPermission")
    private fun showNotification(
        result: QueuePlaylistSaveResult,
        message: String
    ) {
        if (
            Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU &&
            ContextCompat.checkSelfPermission(
                context,
                Manifest.permission.POST_NOTIFICATIONS
            ) != PackageManager.PERMISSION_GRANTED
        ) {
            return
        }

        val notificationManager = NotificationManagerCompat.from(context)

        if (!notificationManager.areNotificationsEnabled()) {
            return
        }

        ensureChannel()

        val launchIntent = context.packageManager
            .getLaunchIntentForPackage(context.packageName)
            ?.apply {
                flags = Intent.FLAG_ACTIVITY_NEW_TASK or
                        Intent.FLAG_ACTIVITY_SINGLE_TOP
            }

        val contentIntent = launchIntent?.let { launchIntent ->
            PendingIntent.getActivity(
                context,
                0,
                launchIntent,
                PendingIntent.FLAG_UPDATE_CURRENT or
                        PendingIntent.FLAG_IMMUTABLE
            )
        }

        val notification = NotificationCompat.Builder(context, CHANNEL_ID)
            .setSmallIcon(R.drawable.ic_notification)
            .setContentTitle(
                context.getString(
                    if (result.succeeded) {
                        R.string.playlist_saved_title
                    } else {
                        R.string.playlist_save_failed_title
                    }
                )
            )
            .setContentText(message)
            .setStyle(
                NotificationCompat.BigTextStyle()
                    .bigText(message)
            )
            .setContentIntent(contentIntent)
            .setAutoCancel(true)
            .setCategory(NotificationCompat.CATEGORY_STATUS)
            .setVisibility(NotificationCompat.VISIBILITY_PRIVATE)
            .setPriority(NotificationCompat.PRIORITY_DEFAULT)
            .setDefaults(NotificationCompat.DEFAULT_ALL)
            .build()

        try {
            notificationManager.notify(
                "playlist-save-${UUID.randomUUID()}",
                NOTIFICATION_ID,
                notification
            )
        } catch (_: SecurityException) {
        }
    }

    private fun ensureChannel() {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) {
            return
        }

        val manager = context.getSystemService(
            NotificationManager::class.java
        )

        val channel = NotificationChannel(
            CHANNEL_ID,
            context.getString(R.string.playlist_results_channel),
            NotificationManager.IMPORTANCE_DEFAULT
        ).apply {
            enableVibration(true)
        }

        manager.createNotificationChannel(channel)
    }

    private companion object {
        const val CHANNEL_ID = "playlist_save_results"
        const val NOTIFICATION_ID = 1
    }
}
