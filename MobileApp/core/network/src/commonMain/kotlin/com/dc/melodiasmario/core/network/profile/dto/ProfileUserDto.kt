package com.dc.melodiasmario.core.network.profile.dto

import com.dc.melodiasmario.core.model.profile.ProfileUser
import kotlinx.serialization.Serializable

@Serializable
data class ProfileUserDto(
    val discordUserId: String,
    val username: String,
    val displayName: String,
    val avatarUrl: String?,
    val isDiscordConnected: Boolean,
    val isActive: Boolean,
) {
    fun toDomain() = ProfileUser(
        id = discordUserId,
        displayName = displayName,
        username = username,
        provider = "Discord",
        avatarUrl = avatarUrl,
        isActive = isActive,
        isDiscordConnected = isDiscordConnected,
    )
}