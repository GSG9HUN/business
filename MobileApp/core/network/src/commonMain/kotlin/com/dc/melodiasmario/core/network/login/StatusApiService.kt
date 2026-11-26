package com.dc.melodiasmario.core.network.login

import com.dc.melodiasmario.core.common.AppConstants
import io.ktor.client.HttpClient
import io.ktor.client.plugins.expectSuccess
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
     * Returns whether the API responds with a successful status; transport failures still propagate.
     */
    suspend fun isApiOnline(): Boolean {
        return httpClient.get(urlString = "$baseUrl/status") {
            expectSuccess = false
        }.status.value in 200..299
    }
}
