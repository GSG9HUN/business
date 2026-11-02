package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileData
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes get profile as a domain operation.
 */
@Factory
class GetProfileUseCase(
    private val profileRepository: ProfileRepository
) {
    /**
     * Executes the get profile domain operation.
     */
    operator fun invoke(): Flow<Resource<ProfileData>> = flow {
        emit(value = Resource.Loading)

        try {
            val profileData = profileRepository.getProfile()
            emit(value = Resource.Success(data = profileData))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
