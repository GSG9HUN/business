package com.dc.melodiasmario.core.data.profile

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileData
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings
import com.dc.melodiasmario.core.network.profile.ProfileRemoteDataSource
import com.dc.melodiasmario.core.network.profile.dto.toDomain
import com.dc.melodiasmario.core.network.profile.dto.toProfileSettingsData
import com.dc.melodiasmario.core.network.profile.dto.toUpdateProfileSettingsDto
import org.koin.core.annotation.Single

@Single(binds = [ProfileRepository::class])
class ProfileRepositoryImpl(
    private val profileRemoteDataSource: ProfileRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider,
) : ProfileRepository {
    override suspend fun getProfile(): ProfileData {
        return profileRemoteDataSource.getProfile(
            accessToken = authorizedSessionProvider.getValidSession(),
        ).toDomain()
    }

    override suspend fun updateProfileSettings(userSettings: UserSettings): ProfileSettingsData {
        return profileRemoteDataSource.updateProfileSettings(
            accessToken = authorizedSessionProvider.getValidSession(),
            userSettings = userSettings.toUpdateProfileSettingsDto(),
        ).toProfileSettingsData()
    }
}
