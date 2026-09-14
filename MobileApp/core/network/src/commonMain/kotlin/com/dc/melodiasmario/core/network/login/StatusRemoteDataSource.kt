package com.dc.melodiasmario.core.network.login

interface StatusRemoteDataSource {
    suspend fun isApiOnline(): Boolean
}
