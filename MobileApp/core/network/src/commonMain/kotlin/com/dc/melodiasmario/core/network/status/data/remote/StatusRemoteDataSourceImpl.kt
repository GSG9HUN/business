package com.dc.melodiasmario.core.network.status.data.remote

import org.koin.core.annotation.Single

@Single(binds = [StatusRemoteDataSource::class])
class StatusRemoteDataSourceImpl(
    private val statusApiService: StatusApiService,
) : StatusRemoteDataSource {
    override suspend fun isApiOnline(): Boolean {
        return statusApiService.isApiOnline()
    }
}
