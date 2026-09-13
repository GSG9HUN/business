package com.dc.melodiasmario.core.domain.login.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.model.login.ApiConnectionStatus
import com.dc.melodiasmario.core.domain.login.StatusRepository
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class CheckApiStatusUseCase(
    private val statusRepository: StatusRepository,
) {
    operator fun invoke(): Flow<Resource<ApiConnectionStatus>> = flow {
        emit(Resource.Loading)

        try {
            val status = statusRepository.getApiStatus()
            emit(Resource.Success(status))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}
