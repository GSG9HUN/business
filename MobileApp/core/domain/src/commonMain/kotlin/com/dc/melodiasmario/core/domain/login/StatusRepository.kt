package com.dc.melodiasmario.core.domain.login

import com.dc.melodiasmario.core.model.login.ApiConnectionStatus

/**
 * Defines status data operations independently of their concrete data sources.
 */
interface StatusRepository {
    /**
     * Retrieves the API availability status.
     */
    suspend fun getApiStatus(): ApiConnectionStatus
}
