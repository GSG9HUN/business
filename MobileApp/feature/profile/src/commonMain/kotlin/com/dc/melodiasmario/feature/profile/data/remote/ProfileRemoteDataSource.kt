package com.dc.melodiasmario.feature.profile.data.remote

import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileSettingsDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.UpdateProfileSettingsDto

interface ProfileRemoteDataSource {
    suspend fun getProfile(accessToken: String): ProfileDto
    suspend fun updateProfileSettings(accessToken: String, userSettings: UpdateProfileSettingsDto): ProfileSettingsDto
}
