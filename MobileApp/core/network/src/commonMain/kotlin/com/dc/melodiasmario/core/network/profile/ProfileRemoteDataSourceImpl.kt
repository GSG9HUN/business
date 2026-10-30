package com.dc.melodiasmario.core.network.profile

import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.network.profile.dto.UpdateProfileSettingsDto
import org.koin.core.annotation.Single

/**
 * Adapts remote profile responses to domain-facing data.
 */
@Single(binds = [ProfileRemoteDataSource::class])
class ProfileRemoteDataSourceImpl(
    private val profileApiService: ProfileApiService
) : ProfileRemoteDataSource {
    override suspend fun getProfile(accessToken: String): ProfileData {
        return profileApiService.getProfile(accessToken = accessToken)
            .toDomain()
    }

    override suspend fun updateProfileSettings(
        accessToken: String,
        userSettings: UpdateProfileSettingsDto
    ): ProfileSettingsData {
        return profileApiService.updateProfileSettings(
            accessToken = accessToken,
            userSettings = userSettings
        )
            .toProfileSettingsData()
    }
}
