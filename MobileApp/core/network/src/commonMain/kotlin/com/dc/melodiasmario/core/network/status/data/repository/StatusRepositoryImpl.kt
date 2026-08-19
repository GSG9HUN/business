package com.dc.melodiasmario.core.network.status.data.repository

import com.dc.melodiasmario.core.network.status.data.remote.StatusRemoteDataSource
import com.dc.melodiasmario.core.network.status.domain.model.ApiConnectionStatus
import com.dc.melodiasmario.core.network.status.domain.repository.StatusRepository
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
