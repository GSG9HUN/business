package com.dc.melodiasmario.core.domain.login

import com.dc.melodiasmario.core.model.login.ApiConnectionStatus

interface StatusRepository {
    suspend fun getApiStatus(): ApiConnectionStatus
}