package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileData
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

@Factory
class GetProfileUseCase(
    private val profileRepository: ProfileRepository,
) {
    operator fun invoke(): Flow<Resource<ProfileData>> = flow {
        emit(Resource.Loading)

        try {
            val profileData = profileRepository.getProfile()
            emit(Resource.Success(profileData))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
