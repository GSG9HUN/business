package com.dc.melodiasmario.core.settings.domain.model

data class UserSettings(
    val languageCode: String,
    val theme: String,
    val hapticFeedbackEnabled: Boolean,
    val soundEffectsEnabled: Boolean,
    val telemetryEnabled: Boolean,
) {
    companion object {
        val Default = UserSettings(
            languageCode = "en",
            theme = "system",
            hapticFeedbackEnabled = false,
            soundEffectsEnabled = true,
            telemetryEnabled = false,
        )
    }
}