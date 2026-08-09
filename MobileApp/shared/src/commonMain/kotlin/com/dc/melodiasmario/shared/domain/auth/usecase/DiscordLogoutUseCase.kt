package com.dc.melodiasmario.shared.domain.auth.usecase

import org.koin.core.annotation.Single
import com.dc.melodiasmario.shared.core.Resource
import com.dc.melodiasmario.shared.domain.auth.repository.AuthRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow

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