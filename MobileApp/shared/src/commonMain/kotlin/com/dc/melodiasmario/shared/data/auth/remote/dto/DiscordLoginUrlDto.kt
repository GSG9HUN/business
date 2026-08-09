package com.dc.melodiasmario.shared.data.auth.remote.dto

import com.dc.melodiasmario.shared.domain.auth.model.DiscordLoginUrl
import kotlinx.serialization.Serializable

@Serializable
data class DiscordLoginUrlDto(val authorizeUrl: String)

fun DiscordLoginUrlDto.toDomain() = DiscordLoginUrl(authorizeUrl)
