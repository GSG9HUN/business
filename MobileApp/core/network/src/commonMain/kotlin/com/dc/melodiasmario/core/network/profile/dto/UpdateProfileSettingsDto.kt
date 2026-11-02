package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.serialization.Serializable

/**
 * Represents update profile settings dto in the API transport contract.
 *
 * @property languageCode Language code.
 * @property theme Theme.
 * @property hapticFeedbackEnabled Haptic feedback enabled.
 * @property soundEffectsEnabled Sound effects enabled.
 * @property telemetryEnabled Telemetry enabled.
 */
@Serializable
data class UpdateProfileSettingsDto(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean
)

/**
 * Maps this value to update profile settings dto.
 */
fun UserSettings.toUpdateProfileSettingsDto(): UpdateProfileSettingsDto = UpdateProfileSettingsDto(
    languageCode = languageCode,
    theme = theme,
    hapticFeedbackEnabled = hapticFeedbackEnabled,
    soundEffectsEnabled = soundEffectsEnabled,
    telemetryEnabled = telemetryEnabled
)
