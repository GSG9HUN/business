package com.dc.melodiasmario.core.data.currentuser

import com.dc.melodiasmario.core.data.auth.AuthorizedSessionProvider
import com.dc.melodiasmario.core.domain.currentuser.CurrentUserRepository
import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import com.dc.melodiasmario.core.network.currentuser.CurrentUserRemoteDataSource
import org.koin.core.annotation.Single

@Single(binds = [CurrentUserRepository::class])
class CurrentUserRepositoryImpl(
    private val currentUserRemoteDataSource: CurrentUserRemoteDataSource,
    private val authorizedSessionProvider: AuthorizedSessionProvider
) : CurrentUserRepository {
    override suspend fun getCurrentUser(): CurrentUser {
        val accessToken = authorizedSessionProvider.getValidSession()

        return currentUserRemoteDataSource.getCurrentUser(accessToken = accessToken)
    }
}