package com.dc.melodiasmario.core.model.profile

import com.dc.melodiasmario.core.model.settings.UserSettings

/**
 * Represents profile settings data shared between application layers.
 *
 * @property userSettings User settings.
 * @property settingsUpdatedAtUtc Settings updated at utc.
 */
data class ProfileSettingsData(
    val userSettings: UserSettings,
    val settingsUpdatedAtUtc: String
)
