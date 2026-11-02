package com.dc.melodiasmario.core.model.settings

/**
 * Represents user settings shared between application layers.
 *
 * @property languageCode Language code.
 * @property theme Theme.
 * @property hapticFeedbackEnabled Haptic feedback enabled.
 * @property soundEffectsEnabled Sound effects enabled.
 * @property telemetryEnabled Telemetry enabled.
 */
data class UserSettings(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean
) {
    /**
     * Shared values and operations for the enclosing type.
     */
    companion object {
        /**
         * Default exposed by companion.
         */
        val Default: UserSettings = UserSettings(
            languageCode = "en",
            theme = "system",
            hapticFeedbackEnabled = false,
            soundEffectsEnabled = true,
            telemetryEnabled = false
        )
    }
}
