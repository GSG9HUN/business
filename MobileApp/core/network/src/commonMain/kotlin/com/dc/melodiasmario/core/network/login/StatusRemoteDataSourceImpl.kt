package com.dc.melodiasmario.core.network.login

import org.koin.core.annotation.Single

/**
 * Adapts remote status responses to domain-facing data.
 */
@Single(binds = [StatusRemoteDataSource::class])
class StatusRemoteDataSourceImpl(
    private val statusApiService: StatusApiService
) : StatusRemoteDataSource {
    override suspend fun isApiOnline(): Boolean {
        return statusApiService.isApiOnline()
    }
}
