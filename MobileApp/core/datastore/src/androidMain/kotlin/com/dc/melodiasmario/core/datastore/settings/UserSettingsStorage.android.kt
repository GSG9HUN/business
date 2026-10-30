package com.dc.melodiasmario.core.datastore.settings

import android.content.Context
import com.dc.melodiasmario.core.model.settings.UserSettings
import org.koin.core.annotation.Single

/**
 * Provides user settings storage for the settings layer.
 */
@Single
actual class UserSettingsStorage(
    private val context: Context
) {
    private val sharedPreferences =
        context.getSharedPreferences("user_settings", Context.MODE_PRIVATE)

    /**
     * Reads the current user settings.
     */
    actual suspend fun getSettings(): UserSettings? {

        if (!sharedPreferences.contains(KEY_THEME)) {
            return null
        }

        val theme = sharedPreferences.getString(KEY_THEME, UserSettings.Default.theme)
            ?: UserSettings.Default.theme

        val languageCode =
            sharedPreferences.getString(KEY_LANGUAGE_CODE, UserSettings.Default.languageCode)
                ?: UserSettings.Default.languageCode

        val hapticFeedbackEnabled = sharedPreferences.getBoolean(
            KEY_HAPTIC_FEEDBACK_ENABLED,
            UserSettings.Default.hapticFeedbackEnabled
        )

        val soundEffectsEnabled = sharedPreferences.getBoolean(
            KEY_SOUND_EFFECTS_ENABLED,
            UserSettings.Default.soundEffectsEnabled
        )
        val telemetryEnabled = sharedPreferences.getBoolean(
            KEY_TELEMETRY_ENABLED,
            UserSettings.Default.telemetryEnabled
        )

        return UserSettings(
            theme = theme,
            languageCode = languageCode,
            hapticFeedbackEnabled = hapticFeedbackEnabled,
            soundEffectsEnabled = soundEffectsEnabled,
            telemetryEnabled = telemetryEnabled
        )
    }

    /**
     * Persists the supplied user settings.
     */
    actual suspend fun saveSettings(settings: UserSettings): Unit {
        sharedPreferences.edit()
            .putString(KEY_THEME, settings.theme)
            .putString(KEY_LANGUAGE_CODE, settings.languageCode)
            .putBoolean(KEY_HAPTIC_FEEDBACK_ENABLED, settings.hapticFeedbackEnabled)
            .putBoolean(KEY_SOUND_EFFECTS_ENABLED, settings.soundEffectsEnabled)
            .putBoolean(KEY_TELEMETRY_ENABLED, settings.telemetryEnabled)
            .apply()
    }

    /**
     * Clears the state and resources owned by UserSettingsStorage.
     */
    actual suspend fun clear(): Unit {
        sharedPreferences.edit()
            .clear()
            .apply()
    }

    private companion object {
        private const val KEY_THEME = "theme"
        private const val KEY_LANGUAGE_CODE = "language_code"
        private const val KEY_HAPTIC_FEEDBACK_ENABLED = "haptic_feedback_enabled"
        private const val KEY_SOUND_EFFECTS_ENABLED = "sound_effects_enabled"
        private const val KEY_TELEMETRY_ENABLED = "telemetry_enabled"
    }
}
