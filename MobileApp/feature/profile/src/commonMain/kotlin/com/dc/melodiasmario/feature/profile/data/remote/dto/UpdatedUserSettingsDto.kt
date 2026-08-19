package com.dc.melodiasmario.feature.profile.data.remote.dto

import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import kotlinx.serialization.Serializable

@Serializable
data class UpdateUserSettingsDto(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean,
)

fun UserSettings.toUpdateDto() = UpdateUserSettingsDto(
    languageCode = languageCode,
    theme = theme,
    hapticFeedbackEnabled = hapticFeedbackEnabled,
    soundEffectsEnabled = soundEffectsEnabled,
    telemetryEnabled = telemetryEnabled,
)
