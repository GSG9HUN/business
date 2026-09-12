package com.dc.melodiasmario.core.domain.auth.usecase

import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import com.dc.melodiasmario.core.domain.auth.AuthRepository
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
