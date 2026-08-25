package com.dc.melodiasmario.feature.guild.data.remote.currentuser

import com.dc.melodiasmario.core.common.AppConstants
import com.dc.melodiasmario.feature.guild.data.remote.currentuser.dto.CurrentUserDto
import io.ktor.client.HttpClient
import io.ktor.client.call.body
import io.ktor.client.request.get
import io.ktor.client.request.header
import org.koin.core.annotation.Property
import org.koin.core.annotation.Single

@Single
class CurrentUserApiService(
    @Property(AppConstants.Properties.ApiBaseUrl)
    private val baseUrl: String,
    private val client: HttpClient,
) {
    suspend fun getCurrentUser(accessToken: String): CurrentUserDto {
        return client.get("$baseUrl/profile/me") {
            header("Authorization", "Bearer $accessToken")
        }.body()
    }
}