package com.dc.melodiasmario.shared.data.status.repository

import com.dc.melodiasmario.shared.data.status.remote.StatusRemoteDataSource
import com.dc.melodiasmario.shared.domain.status.model.ApiConnectionStatus
import com.dc.melodiasmario.shared.domain.status.repository.StatusRepository
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
