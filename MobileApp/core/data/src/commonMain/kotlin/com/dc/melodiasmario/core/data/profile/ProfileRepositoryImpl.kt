package com.dc.melodiasmario.core.data.profile

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings
import com.dc.melodiasmario.core.network.profile.ProfileRemoteDataSource
import com.dc.melodiasmario.core.network.profile.dto.toUpdateProfileSettingsDto
import org.koin.core.annotation.Single

@Single(binds = [ProfileRepository::class])
class ProfileRepositoryImpl(
    private val profileRemoteDataSource: ProfileRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider,
) : ProfileRepository {
    override suspend fun getProfile(): ProfileData {
        val accessToken = authorizedSessionProvider.getValidSession()
        return profileRemoteDataSource.getProfile(
            accessToken = accessToken,
        )
    }

    override suspend fun updateProfileSettings(userSettings: UserSettings): ProfileSettingsData {
        val accessToken = authorizedSessionProvider.getValidSession()
        val userSettings = userSettings.toUpdateProfileSettingsDto()

        return profileRemoteDataSource.updateProfileSettings(
            accessToken = accessToken,
            userSettings = userSettings,
        )
    }
}
