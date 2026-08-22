package com.dc.melodiasmario.feature.profile.domain.model

import com.dc.melodiasmario.core.settings.domain.model.UserSettings

data class ProfileData(
    val user: ProfileUser,
    val userSettings: UserSettings,
    val settingsUpdatedAtUtc: String,
)
