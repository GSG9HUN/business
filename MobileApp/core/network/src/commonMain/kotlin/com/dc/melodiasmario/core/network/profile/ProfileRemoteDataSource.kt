package com.dc.melodiasmario.core.network.profile

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.network.profile.dto.UpdateProfileSettingsDto

interface ProfileRemoteDataSource {
    suspend fun getProfile(accessToken: String): ProfileData
    suspend fun updateProfileSettings(
        accessToken: String,
        userSettings: UpdateProfileSettingsDto
    ): ProfileSettingsData
}
