package com.dc.melodiasmario.core.auth.domain.usecase

import com.dc.melodiasmario.core.auth.domain.repository.AuthRepository
import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class DiscordLogoutUseCase(
    private val authRepository: AuthRepository,
) {
    operator fun invoke(refreshToken: String): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)

        try {
            authRepository.logout(refreshToken)
            emit(Resource.Success(Unit))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
