package com.dc.melodiasmario.core.model.profile

import com.dc.melodiasmario.core.model.settings.UserSettings

data class ProfileSettingsData(
    val userSettings: UserSettings,
    val settingsUpdatedAtUtc: String,
)
