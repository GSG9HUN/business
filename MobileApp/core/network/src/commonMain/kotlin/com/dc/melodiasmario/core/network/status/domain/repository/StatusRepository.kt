package com.dc.melodiasmario.core.network.status.domain.repository

import com.dc.melodiasmario.core.network.status.domain.model.ApiConnectionStatus

interface StatusRepository {
    suspend fun getApiStatus(): ApiConnectionStatus
}
