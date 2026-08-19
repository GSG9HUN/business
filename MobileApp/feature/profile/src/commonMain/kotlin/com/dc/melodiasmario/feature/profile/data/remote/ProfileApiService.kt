package com.dc.melodiasmario.feature.profile.data.remote

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.feature.profile.data.remote.dto.ProfileDto
import com.dc.melodiasmario.feature.profile.data.remote.dto.UpdateUserSettingsDto
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

    suspend fun updateProfile(
        accessToken: String,
        userSettings: UpdateUserSettingsDto,
    ): ProfileDto {
        return client.patch("$baseUrl/profile/settings") {
            header("Authorization", "Bearer $accessToken")
            contentType(ContentType.Application.Json)
            setBody(userSettings)
        }.body<ProfileDto>()
    }
}
