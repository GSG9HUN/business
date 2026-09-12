package com.dc.melodiasmario.core.network.auth.dto

import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import kotlinx.serialization.Serializable

@Serializable
data class DiscordLoginUrlDto(val authorizeUrl: String)

fun DiscordLoginUrlDto.toDomain() = DiscordLoginUrl(authorizeUrl)
