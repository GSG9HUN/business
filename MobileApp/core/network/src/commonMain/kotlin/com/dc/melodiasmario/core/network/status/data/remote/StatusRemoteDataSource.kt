package com.dc.melodiasmario.core.network.status.data.remote

interface StatusRemoteDataSource {
    suspend fun isApiOnline(): Boolean
}
