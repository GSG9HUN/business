package com.dc.melodiasmario.core.network.currentuser

import com.dc.melodiasmario.core.network.currentuser.dto.toDomain
import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import org.koin.core.annotation.Single

@Single(binds = [CurrentUserRemoteDataSource::class])
class CurrentUserRemoteDataSourceImpl(
    private val currentUserApiService: CurrentUserApiService
): CurrentUserRemoteDataSource {
    override suspend fun getCurrentUser(accessToken: String): CurrentUser {
        return currentUserApiService.getCurrentUser(accessToken = accessToken).toDomain()
    }
}