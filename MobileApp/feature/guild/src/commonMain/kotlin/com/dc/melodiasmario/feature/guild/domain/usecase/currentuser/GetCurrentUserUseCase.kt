package com.dc.melodiasmario.feature.guild.domain.usecase.currentuser

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.feature.guild.domain.model.currentuser.CurrentUser
import com.dc.melodiasmario.feature.guild.domain.repository.currentuser.CurrentUserRepository
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