package com.dc.melodiasmario.core.domain.auth.usecase

import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.domain.auth.AuthRepository
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
