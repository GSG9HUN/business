package com.dc.melodiasmario.core.domain.currentuser.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currentuser.CurrentUserRepository
import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Factory

/**
 * Exposes get current user as a domain operation.
 */
@Factory
class GetCurrentUserUseCase(private val currentUserRepository: CurrentUserRepository) {

    /**
     * Executes the get current user domain operation.
     */
    operator fun invoke(): Flow<Resource<CurrentUser>> = flow {
        emit(value = Resource.Loading)

        try {
            val currentUser = currentUserRepository.getCurrentUser()
            emit(value = Resource.Success(data = currentUser))
        } catch (cancellationException: CancellationException) {
            throw cancellationException
        } catch (exception: Exception) {
            emit(value = Resource.Error(error = exception))
        }
    }
}
