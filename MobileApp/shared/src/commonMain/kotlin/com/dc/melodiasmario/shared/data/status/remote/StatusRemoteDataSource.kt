package com.dc.melodiasmario.shared.data.status.remote

interface StatusRemoteDataSource {
    suspend fun isApiOnline(): Boolean
}
