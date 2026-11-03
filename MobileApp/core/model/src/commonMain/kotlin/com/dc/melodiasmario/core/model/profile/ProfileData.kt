package com.dc.melodiasmario.core.model.profile

import com.dc.melodiasmario.core.model.settings.UserSettings

/**
 * Represents profile data shared between application layers.
 *
 * @property user User.
 * @property userSettings User settings.
 * @property settingsUpdatedAtUtc Settings updated at utc.
 */
data class ProfileData(
    val user: ProfileUser,
    val userSettings: UserSettings,
    val settingsUpdatedAtUtc: String
)
