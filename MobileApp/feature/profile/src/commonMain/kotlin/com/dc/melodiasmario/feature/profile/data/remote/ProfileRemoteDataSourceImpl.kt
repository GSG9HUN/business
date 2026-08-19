package com.dc.melodiasmario.feature.profile.data.remote

import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.UpdateUserSettingsDto
import org.koin.core.annotation.Single

@Single(binds = [ProfileRemoteDataSource::class])
class ProfileRemoteDataSourceImpl(
    private val profileApiService: ProfileApiService,
) : ProfileRemoteDataSource {
    override suspend fun getProfile(accessToken: String): ProfileDto {
        return profileApiService.getProfile(accessToken = accessToken)
    }

    override suspend fun updateProfile(
        accessToken: String,
        userSettings: UpdateUserSettingsDto
    ): ProfileDto {
        return profileApiService.updateProfile(
            accessToken = accessToken,
            userSettings = userSettings,
        )
    }
}
