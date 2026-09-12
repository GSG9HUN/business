package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings
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
