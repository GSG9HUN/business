package com.dc.melodiasmario.core.commonui.feedback

import platform.UIKit.UINotificationFeedbackGenerator
import platform.UIKit.UINotificationFeedbackType

/**
 * Provides feedback vibrator for the feedback layer.
 */
actual class FeedbackVibrator {
    private val generator = UINotificationFeedbackGenerator()

    /**
     * Requests platform haptic feedback for the supplied interaction.
     */
    actual fun vibrate() {
        generator.prepare()
        generator.notificationOccurred(
            UINotificationFeedbackType.UINotificationFeedbackTypeSuccess
        )
    }
}
