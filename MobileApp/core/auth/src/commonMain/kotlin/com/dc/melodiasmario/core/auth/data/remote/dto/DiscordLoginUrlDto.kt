package com.dc.melodiasmario.core.auth.data.remote.dto

import com.dc.melodiasmario.core.auth.domain.model.DiscordLoginUrl
import kotlinx.serialization.Serializable

@Serializable
data class DiscordLoginUrlDto(val authorizeUrl: String)

fun DiscordLoginUrlDto.toDomain() = DiscordLoginUrl(authorizeUrl)
