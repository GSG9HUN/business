package com.dc.melodiasmario.core.commonui.feedback

import platform.UIKit.UINotificationFeedbackGenerator
import platform.UIKit.UINotificationFeedbackType

actual class FeedbackVibrator {
    private val generator = UINotificationFeedbackGenerator()

    actual fun vibrate() {
        generator.prepare()
        generator.notificationOccurred(
            UINotificationFeedbackType.UINotificationFeedbackTypeSuccess,
        )
    }
}