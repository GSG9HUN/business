package com.dc.melodiasmario.core.network.login

import com.dc.melodiasmario.core.common.AppConstants
import io.ktor.client.HttpClient
import io.ktor.client.request.get
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends status API requests through the configured HTTP client.
 */
@Single
class StatusApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Checks whether the API is available.
     */
    suspend fun isApiOnline(): Boolean {
        return httpClient.get(urlString = "$baseUrl/status").status.value in 200..299
    }
}
