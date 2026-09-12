package com.dc.melodiasmario.core.network.currentuser.dto

import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import kotlinx.serialization.Serializable

@Serializable
data class CurrentUserDto(
    val discordUserId: String,
    val displayName: String,
    val username: String,
    val avatarUrl: String?
)

fun CurrentUserDto.toDomain() = CurrentUser(
    id = discordUserId,
    displayName = displayName,
    username = username,
    avatarUrl = avatarUrl
)
