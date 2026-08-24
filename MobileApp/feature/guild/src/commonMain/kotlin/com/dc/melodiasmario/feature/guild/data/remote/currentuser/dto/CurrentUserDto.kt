package com.dc.melodiasmario.feature.guild.data.remote.currentuser.dto

import com.dc.melodiasmario.feature.guild.domain.model.currentuser.CurrentUser
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
