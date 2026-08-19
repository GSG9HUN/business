package com.dc.melodiasmario.feature.profile.data.remote

import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.UpdateUserSettingsDto

interface ProfileRemoteDataSource {
    suspend fun getProfile(accessToken: String): ProfileDto
    suspend fun updateProfile(accessToken: String, userSettings: UpdateUserSettingsDto): ProfileDto
}
