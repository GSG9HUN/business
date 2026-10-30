package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileSettingsData
import com.dc.melodiasmario.core.model.settings.UserSettings
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes update profile settings as a domain operation.
 */
@Factory
class UpdateProfileSettingsUseCase(
    private val profileRepository: ProfileRepository
) {
    /**
     * Executes the update profile settings domain operation.
     */
    operator fun invoke(userSettings: UserSettings): Flow<Resource<ProfileSettingsData>> = flow {
        emit(value = Resource.Loading)

        try {
            val profileSettingsData = profileRepository.updateProfileSettings(userSettings = userSettings)
            emit(value = Resource.Success(data = profileSettingsData))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
