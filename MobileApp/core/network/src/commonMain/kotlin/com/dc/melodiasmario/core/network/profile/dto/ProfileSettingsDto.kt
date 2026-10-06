package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.serialization.Serializable

@Serializable
data class ProfileSettingsDto(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean,
    val updatedAtUtc: String,
) {
    fun toDomain() = UserSettings(
        languageCode = languageCode,
        theme = theme,
        hapticFeedbackEnabled = hapticFeedbackEnabled,
        soundEffectsEnabled = soundEffectsEnabled,
        telemetryEnabled = telemetryEnabled,
    )

    fun toProfileSettingsData() = ProfileSettingsData(
        userSettings = toDomain(),
        settingsUpdatedAtUtc = updatedAtUtc,
    )
}