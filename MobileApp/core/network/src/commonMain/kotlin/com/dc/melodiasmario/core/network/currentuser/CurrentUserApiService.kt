package com.dc.melodiasmario.core.network.currentuser

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.currentuser.dto.CurrentUserDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends current user API requests through the configured HTTP client.
 */
@Single
class CurrentUserApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the authenticated user account.
     */
    suspend fun getCurrentUser(accessToken: String): CurrentUserDto {
        return httpClient.get(urlString = "$baseUrl/profile/me") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }
}
