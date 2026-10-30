package com.dc.melodiasmario.core.network.profile

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.core.network.profile.dto.ProfileDto
import com.dc.melodiasmario.core.network.profile.dto.ProfileSettingsDto
import com.dc.melodiasmario.core.network.profile.dto.UpdateProfileSettingsDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import io.ktor.client.request.patch
import io.ktor.client.request.setBody
import io.ktor.http.ContentType
import io.ktor.http.contentType
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

/**
 * Sends profile API requests through the configured HTTP client.
 */
@Single
class ProfileApiService(
    @Property(value = AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val httpClient: HttpClient
) {
    /**
     * Retrieves the authenticated user profile and settings.
     */
    suspend fun getProfile(accessToken: String): ProfileDto {
        return httpClient.get(urlString = "$baseUrl/profile") {
            header(key = "Authorization", value = "Bearer $accessToken")
        }
            .body()
    }

    /**
     * Persists the supplied profile-setting changes.
     */
    suspend fun updateProfileSettings(
        accessToken: String,
        userSettings: UpdateProfileSettingsDto
    ): ProfileSettingsDto {
        return httpClient.patch(urlString = "$baseUrl/profile/settings") {
            header(key = "Authorization", value = "Bearer $accessToken")
            contentType(type = ContentType.Application.Json)
            setBody(body = userSettings)
        }
            .body<ProfileSettingsDto>()
    }
}
