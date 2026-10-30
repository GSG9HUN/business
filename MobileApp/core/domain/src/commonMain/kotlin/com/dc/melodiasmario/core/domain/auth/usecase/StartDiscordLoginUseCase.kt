package com.dc.melodiasmario.core.domain.auth.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes start discord login as a domain operation.
 */
@Factory
class StartDiscordLoginUseCase(private val authRepository: AuthRepository) {

    /**
     * Executes the start discord login domain operation.
     */
    operator fun invoke(): Flow<Resource<DiscordLoginUrl>> = flow{
        emit(value = Resource.Loading)

        try {
            val authorizeUrl = authRepository.startDiscordLogin()
            emit(value = Resource.Success(data = authorizeUrl))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
