package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileUser
import kotlinx.serialization.Serializable

/**
 * Represents profile user dto in the API transport contract.
 *
 * @property discordUserId Discord user id.
 * @property username Username.
 * @property displayName Display name.
 * @property avatarUrl Avatar url.
 * @property isDiscordConnected Is discord connected.
 * @property isActive Is active.
 */
@Serializable
data class ProfileUserDto(
    val discordUserId: String,
    val username: String,
    val displayName: String,
    val avatarUrl: String?,
    val isDiscordConnected: Boolean,
    val isActive: Boolean
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): ProfileUser = ProfileUser(
        id = discordUserId,
        displayName = displayName,
        username = username,
        provider = "Discord",
        avatarUrl = avatarUrl,
        isActive = isActive,
        isDiscordConnected = isDiscordConnected
    )
}
