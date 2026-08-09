package com.dc.melodiasmario.shared.data.status.remote

import com.dc.melodiasmario.shared.data.ApiService
import org.koin.core.annotation.Single

@Single(binds = [StatusRemoteDataSource::class])
class StatusRemoteDataSourceImpl(
    private val apiService: ApiService,
) : StatusRemoteDataSource {
    override suspend fun isApiOnline(): Boolean {
        return apiService.isApiOnline()
    }
}