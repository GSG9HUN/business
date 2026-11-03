package com.dc.melodiasmario.core.domain.auth.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes exchange auth ticket as a domain operation.
 */
@Factory
class ExchangeAuthTicketUseCase(
    private val authRepository: AuthRepository
) {
    /**
     * Executes the exchange auth ticket domain operation.
     */
    operator fun invoke(ticket: String): Flow<Resource<AuthSession>> = flow{
        emit(value = Resource.Loading)

        try {
            val newAuthSession = authRepository.exchangeTicket(ticket = ticket)
            emit(value = Resource.Success(data = newAuthSession))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
