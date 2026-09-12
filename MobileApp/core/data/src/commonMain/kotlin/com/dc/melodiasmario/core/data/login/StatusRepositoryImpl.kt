package com.dc.melodiasmario.core.data.login

import com.dc.melodiasmario.core.domain.login.StatusRepository
import com.dc.melodiasmario.core.model.login.ApiConnectionStatus
import com.dc.melodiasmario.core.network.login.StatusRemoteDataSource
import org.koin.core.annotation.Single

@Single(binds = [StatusRepository::class])
class StatusRepositoryImpl(
    private val statusRemoteDataSource: StatusRemoteDataSource,
) : StatusRepository {
    override suspend fun getApiStatus(): ApiConnectionStatus {
        return if (statusRemoteDataSource.isApiOnline()) {
            ApiConnectionStatus.Online
        } else {
            ApiConnectionStatus.Offline
        }
    }
}