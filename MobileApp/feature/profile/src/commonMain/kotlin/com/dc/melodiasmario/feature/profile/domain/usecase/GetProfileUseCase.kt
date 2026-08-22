package com.dc.melodiasmario.feature.profile.domain.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.profile.domain.model.ProfileData
import com.dc.melodiasmario.feature.profile.domain.repository.ProfileRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class GetProfileUseCase(
    private val profileRepository: ProfileRepository,
) {
    operator fun invoke(): Flow<Resource<ProfileData>> = flow {
        emit(Resource.Loading)

        try {
            emit(Resource.Success(profileRepository.getProfile()))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
