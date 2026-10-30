package com.dc.melodiasmario.core.network.login

/**
 * Defines remote status data operations.
 */
interface StatusRemoteDataSource {
    /**
     * Checks whether the API is available.
     */
    suspend fun isApiOnline(): Boolean
}
