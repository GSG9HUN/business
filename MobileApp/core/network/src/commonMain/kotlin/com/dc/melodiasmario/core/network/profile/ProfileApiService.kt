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

@Single
class ProfileApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
    suspend fun getProfile(accessToken: String): ProfileDto {
        return client.get("$baseUrl/profile") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }

    suspend fun updateProfileSettings(
        accessToken: String,
        userSettings: UpdateProfileSettingsDto,
    ): ProfileSettingsDto {
        return client.patch("$baseUrl/profile/settings") {
            header("Authorization", "Bearer $accessToken")
            contentType(ContentType.Application.Json)
            setBody(userSettings)
        }.body<ProfileSettingsDto>()
    }
}
