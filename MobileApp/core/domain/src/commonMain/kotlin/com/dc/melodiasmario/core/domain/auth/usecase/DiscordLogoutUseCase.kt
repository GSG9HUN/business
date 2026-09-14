package com.dc.melodiasmario.core.domain.auth.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class DiscordLogoutUseCase(
    private val authRepository: AuthRepository,
    private val secureAuthSessionStorage: SecureAuthSessionStorage
) {
    operator fun invoke(): Flow<Resource<Unit>> = flow {
        emit(Resource.Loading)

        try {
            val session = secureAuthSessionStorage.getSession()
            val refreshToken = session?.refreshToken ?: throw Exception("No refresh token found")
            authRepository.logout(refreshToken)
            emit(Resource.Success(Unit))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
