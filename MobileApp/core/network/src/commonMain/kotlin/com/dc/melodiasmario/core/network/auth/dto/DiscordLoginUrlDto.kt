package com.dc.melodiasmario.core.network.auth.dto

import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import kotlinx.serialization.Serializable

/**
 * Represents discord login url dto in the API transport contract.
 *
 * @property authorizeUrl Authorize url.
 */
@Serializable
data class DiscordLoginUrlDto(
    val authorizeUrl: String
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): DiscordLoginUrl = DiscordLoginUrl(authorizeUrl = authorizeUrl)
}
