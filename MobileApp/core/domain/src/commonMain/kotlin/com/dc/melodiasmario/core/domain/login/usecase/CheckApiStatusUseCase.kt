package com.dc.melodiasmario.core.domain.login.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.login.StatusRepository
import com.dc.melodiasmario.core.model.login.ApiConnectionStatus
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes check api status as a domain operation.
 */
@Factory
class CheckApiStatusUseCase(
    private val statusRepository: StatusRepository
) {
    /**
     * Executes the check api status domain operation.
     */
    operator fun invoke(): Flow<Resource<ApiConnectionStatus>> = flow {
        emit(value = Resource.Loading)

        try {
            val status = statusRepository.getApiStatus()
            emit(value = Resource.Success(data = status))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
