package com.dc.melodiasmario.core.domain.profile.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.profile.ProfileRepository
import com.dc.melodiasmario.core.model.profile.ProfileData
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
