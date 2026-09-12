package com.dc.melodiasmario.core.network.profile

import com.dc.melodiasmario.core.network.profile.dto.ProfileDto
import com.dc.melodiasmario.core.network.profile.dto.ProfileSettingsDto
import com.dc.melodiasmario.core.network.profile.dto.UpdateProfileSettingsDto

interface ProfileRemoteDataSource {
    suspend fun getProfile(accessToken: String): ProfileDto
    suspend fun updateProfileSettings(accessToken: String, userSettings: UpdateProfileSettingsDto): ProfileSettingsDto
}
