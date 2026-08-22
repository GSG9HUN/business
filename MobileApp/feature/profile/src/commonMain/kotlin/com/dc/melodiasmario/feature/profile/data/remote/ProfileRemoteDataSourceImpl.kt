package com.dc.melodiasmario.feature.profile.data.remote

import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileSettingsDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.UpdateProfileSettingsDto
import org.koin.core.annotation.Single

@Single(binds = [ProfileRemoteDataSource::class])
class ProfileRemoteDataSourceImpl(
    private val profileApiService: ProfileApiService,
) : ProfileRemoteDataSource {
    override suspend fun getProfile(accessToken: String): ProfileDto {
        return profileApiService.getProfile(accessToken = accessToken)
    }

    override suspend fun updateProfileSettings(
        accessToken: String,
        userSettings: UpdateProfileSettingsDto
    ): ProfileSettingsDto {
        return profileApiService.updateProfileSettings(
            accessToken = accessToken,
            userSettings = userSettings,
        )
    }
}
