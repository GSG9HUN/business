package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.serialization.Serializable

/**
 * Represents profile settings dto in the API transport contract.
 *
 * @property languageCode Language code.
 * @property theme Theme.
 * @property hapticFeedbackEnabled Haptic feedback enabled.
 * @property soundEffectsEnabled Sound effects enabled.
 * @property telemetryEnabled Telemetry enabled.
 * @property updatedAtUtc Updated at utc.
 */
@Serializable
data class ProfileSettingsDto(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean,
    val updatedAtUtc: String
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): UserSettings = UserSettings(
        languageCode = languageCode,
        theme = theme,
        hapticFeedbackEnabled = hapticFeedbackEnabled,
        soundEffectsEnabled = soundEffectsEnabled,
        telemetryEnabled = telemetryEnabled
    )

    /**
     * Maps this value to profile settings data.
     */
    fun toProfileSettingsData(): ProfileSettingsData = ProfileSettingsData(
        userSettings = toDomain(),
        settingsUpdatedAtUtc = updatedAtUtc
    )
}
