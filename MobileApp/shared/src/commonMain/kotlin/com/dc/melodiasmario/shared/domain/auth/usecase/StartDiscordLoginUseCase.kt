package com.dc.melodiasmario.shared.domain.auth.usecase

import com.dc.melodiasmario.shared.core.Resource
import com.dc.melodiasmario.shared.domain.auth.model.DiscordLoginUrl
import com.dc.melodiasmario.shared.domain.auth.repository.AuthRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class StartDiscordLoginUseCase(private val authRepository: AuthRepository) {

    operator fun invoke(): Flow<Resource<DiscordLoginUrl>> = flow{
        emit(Resource.Loading)

        try {
            val authorizeUrl = authRepository.startDiscordLogin()
            emit(Resource.Success(authorizeUrl))
        }catch (e: Exception){
            emit(Resource.Error(e))
        }
    }
}