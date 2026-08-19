package com.dc.melodiasmario.core.auth.domain.usecase

import com.dc.melodiasmario.core.auth.domain.model.AuthSession
import com.dc.melodiasmario.core.auth.domain.repository.AuthRepository
import com.dc.melodiasmario.core.common.Resource
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class ExchangeAuthTicketUseCase(
    private val authRepository: AuthRepository
) {
    operator fun invoke(ticket: String): Flow<Resource<AuthSession>> = flow{
        emit(Resource.Loading)

        try {
            val newAuthSession = authRepository.exchangeTicket(ticket = ticket)
            emit(Resource.Success(newAuthSession))
        }catch (e: Exception){
            emit(Resource.Error(e))
        }
    }
}
