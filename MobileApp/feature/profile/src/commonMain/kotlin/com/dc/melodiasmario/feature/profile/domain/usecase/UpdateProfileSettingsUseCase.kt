package com.dc.melodiasmario.feature.profile.domain.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.settings.domain.model.UserSettings
import com.dc.melodiasmario.feature.profile.domain.model.ProfileSettingsData
import com.dc.melodiasmario.feature.profile.domain.repository.ProfileRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class UpdateProfileSettingsUseCase(
    private val profileRepository: ProfileRepository
) {
    operator fun invoke(userSettings: UserSettings): Flow<Resource<ProfileSettingsData>> = flow {
        emit(Resource.Loading)

        try {
            emit(Resource.Success(profileRepository.updateProfileSettings(userSettings = userSettings)))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
