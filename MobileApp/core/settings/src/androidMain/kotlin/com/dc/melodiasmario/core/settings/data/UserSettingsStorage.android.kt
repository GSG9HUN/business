package com.dc.melodiasmario.core.settings.data

import android.content.Context
import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import org.koin.core.annotation.Single

@Single
actual class UserSettingsStorage(
    private val context: Context
) {
    private val sharedPreferences =
        context.getSharedPreferences("user_settings", Context.MODE_PRIVATE)

    actual suspend fun getSettings(): UserSettings? {

        if (!sharedPreferences.contains(KEY_THEME)) return null

        val theme = sharedPreferences.getString(KEY_THEME, UserSettings.Default.theme)
            ?: UserSettings.Default.theme

        val languageCode =
            sharedPreferences.getString(KEY_LANGUAGE_CODE, UserSettings.Default.languageCode)
                ?: UserSettings.Default.languageCode

        val hapticFeedbackEnabled = sharedPreferences.getBoolean(
            KEY_HAPTIC_FEEDBACK_ENABLED, UserSettings.Default.hapticFeedbackEnabled
        )

        val soundEffectsEnabled = sharedPreferences.getBoolean(
            KEY_SOUND_EFFECTS_ENABLED, UserSettings.Default.soundEffectsEnabled
        )
        val telemetryEnabled = sharedPreferences.getBoolean(
            KEY_TELEMETRY_ENABLED, UserSettings.Default.telemetryEnabled
        )

        return UserSettings(
            theme = theme,
            languageCode = languageCode,
            hapticFeedbackEnabled = hapticFeedbackEnabled,
            soundEffectsEnabled = soundEffectsEnabled,
            telemetryEnabled = telemetryEnabled
        )
    }

    actual suspend fun saveSettings(settings: UserSettings) {
        sharedPreferences.edit().putString(KEY_THEME, settings.theme)
            .putString(KEY_LANGUAGE_CODE, settings.languageCode)
            .putBoolean(KEY_HAPTIC_FEEDBACK_ENABLED, settings.hapticFeedbackEnabled)
            .putBoolean(KEY_SOUND_EFFECTS_ENABLED, settings.soundEffectsEnabled)
            .putBoolean(KEY_TELEMETRY_ENABLED, settings.telemetryEnabled).apply()
    }

    actual suspend fun clear() {
        sharedPreferences.edit().clear().apply()
    }

    private companion object {
        private const val KEY_THEME = "theme"
        private const val KEY_LANGUAGE_CODE = "language_code"
        private const val KEY_HAPTIC_FEEDBACK_ENABLED = "haptic_feedback_enabled"
        private const val KEY_SOUND_EFFECTS_ENABLED = "sound_effects_enabled"
        private const val KEY_TELEMETRY_ENABLED = "telemetry_enabled"
    }
}