package com.dc.melodiasmario.feature.guild.data.repository.currentuser

import com.dc.melodiasmario.core.auth.data.session.AuthorizedSessionProvider
import com.dc.melodiasmario.feature.guild.data.remote.currentuser.CurrentUserRemoteDataSource
import com.dc.melodiasmario.feature.guild.domain.model.currentuser.CurrentUser
import com.dc.melodiasmario.feature.guild.domain.repository.currentuser.CurrentUserRepository
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