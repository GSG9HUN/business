package com.dc.melodiasmario.core.commonui.feedback

/**
 * Provides feedback vibrator for the feedback layer.
 */
expect class FeedbackVibrator {
    /**
     * Requests platform haptic feedback for the supplied interaction.
     */
    fun vibrate(): Unit
}
