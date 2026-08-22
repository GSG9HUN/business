package com.dc.melodiasmario.feature.profile.data.repository

import com.dc.melodiasmario.core.auth.data.session.AuthorizedSessionProvider
import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.feature.profile.data.remote.ProfileRemoteDataSource
import com.dc.melodiasmario.feature.profile.data.remote.dto.toDomain
import com.dc.melodiasmario.feature.profile.data.remote.dto.toProfileSettingsData
import com.dc.melodiasmario.feature.profile.data.remote.dto.toUpdateProfileSettingsDto
import com.dc.melodiasmario.feature.profile.domain.model.ProfileData
import com.dc.melodiasmario.feature.profile.domain.model.ProfileSettingsData
import com.dc.melodiasmario.feature.profile.domain.repository.ProfileRepository
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
