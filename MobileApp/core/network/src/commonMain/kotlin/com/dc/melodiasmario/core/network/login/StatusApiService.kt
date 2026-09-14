package com.dc.melodiasmario.core.network.login

import com.dc.melodiasmario.core.common.AppConstants
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

@Single
class StatusApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
    suspend fun isApiOnline(): Boolean {
        return client.get("$baseUrl/status").status.value in 200..299
    }
}
