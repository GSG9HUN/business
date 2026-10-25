package com.dc.melodiasmario.core.commonui.feedback
import android.content.Context
import android.media.AudioAttributes
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import android.provider.Settings

actual class FeedbackVibrator(
    context: Context,
) {
    private val applicationContext = context.applicationContext

    @Suppress("DEPRECATION")
    private val vibrator: Vibrator? =
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            applicationContext
                .getSystemService(VibratorManager::class.java)
                ?.defaultVibrator
        } else {
            applicationContext.getSystemService(
                Context.VIBRATOR_SERVICE,
            ) as? Vibrator
        }

    private val audioAttributes = AudioAttributes.Builder()
        .setUsage(AudioAttributes.USAGE_ASSISTANCE_SONIFICATION)
        .setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION)
        .build()

    @Suppress("DEPRECATION")
    actual fun vibrate() {
        val vibrator = vibrator ?: return

        if (!vibrator.hasVibrator()) {
            return
        }

        val systemHapticsEnabled = Settings.System.getInt(
            applicationContext.contentResolver,
            Settings.System.HAPTIC_FEEDBACK_ENABLED,
            1
        ) != 0

        if (!systemHapticsEnabled) {
            return
        }

        val timings = longArrayOf(
            0L,
            220L,
            140L,
            220L,
        )

        try {
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
                vibrator.vibrate(VibrationEffect.createWaveform(timings, -1), audioAttributes)
            } else {
                vibrator.vibrate(timings, -1, audioAttributes)
            }
        } catch (_: SecurityException) {
        }
    }
}