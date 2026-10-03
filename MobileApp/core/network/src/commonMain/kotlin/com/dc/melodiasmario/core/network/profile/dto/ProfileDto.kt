package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.profile.ProfileUser
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.serialization.Serializable

@Serializable
data class ProfileDto(
    val user: ProfileUserDto,
    val userSettings: ProfileSettingsDto,
){
    fun toDomain() = ProfileData(
        user = user.toDomain(),
        userSettings = userSettings.toDomain(),
        settingsUpdatedAtUtc = userSettings.updatedAtUtc,
    )
}
