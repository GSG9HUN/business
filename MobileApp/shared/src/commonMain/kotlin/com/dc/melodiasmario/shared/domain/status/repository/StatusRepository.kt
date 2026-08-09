package com.dc.melodiasmario.shared.domain.status.repository

import com.dc.melodiasmario.shared.domain.status.model.ApiConnectionStatus

interface StatusRepository {
    suspend fun getApiStatus(): ApiConnectionStatus
}
