package com.dc.melodiasmario.core.auth.domain.usecase

import com.dc.melodiasmario.core.auth.domain.model.DiscordLoginUrl
import com.dc.melodiasmario.core.auth.domain.repository.AuthRepository
import com.dc.melodiasmario.core.common.Resource
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