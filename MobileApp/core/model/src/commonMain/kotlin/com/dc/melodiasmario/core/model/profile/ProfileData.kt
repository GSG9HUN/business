package com.dc.melodiasmario.core.model.profile

import com.dc.melodiasmario.core.model.settings.UserSettings

data class ProfileData(
    val user: ProfileUser,
    val userSettings: UserSettings,
    val settingsUpdatedAtUtc: String,
)
