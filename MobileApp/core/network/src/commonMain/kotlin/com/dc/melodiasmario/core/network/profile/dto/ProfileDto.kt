package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileData
import kotlinx.serialization.Serializable

/**
 * Represents profile dto in the API transport contract.
 *
 * @property user User.
 * @property userSettings User settings.
 */
@Serializable
data class ProfileDto(
    val user: ProfileUserDto,
    val userSettings: ProfileSettingsDto
){
    /**
     * Maps this value to domain.
     */
    fun toDomain(): ProfileData = ProfileData(
        user = user.toDomain(),
        userSettings = userSettings.toDomain(),
        settingsUpdatedAtUtc = userSettings.updatedAtUtc
    )
}
