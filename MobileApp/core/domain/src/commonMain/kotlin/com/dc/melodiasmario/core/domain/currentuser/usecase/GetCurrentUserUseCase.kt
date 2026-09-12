package com.dc.melodiasmario.core.domain.currentuser.usecase

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.currentuser.CurrentUserRepository
import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import kotlinx.coroutines.flow.Flow
import kotlinx.coroutines.flow.flow
import org.koin.core.annotation.Single

@Single
class GetCurrentUserUseCase(private val currentUserRepository: CurrentUserRepository) {

    operator fun invoke(): Flow<Resource<CurrentUser>> = flow {
        emit(Resource.Loading)

        try {
            val currentUser = currentUserRepository.getCurrentUser()
            emit(Resource.Success(currentUser))
        } catch (e: Exception) {
            emit(Resource.Error(e))
        }
    }
}