package com.dc.melodiasmario.core.network.currentuser.dto

import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import kotlinx.serialization.Serializable

/**
 * Represents current user dto in the API transport contract.
 *
 * @property discordUserId Discord user id.
 * @property displayName Display name.
 * @property username Username.
 * @property avatarUrl Avatar url.
 */
@Serializable
data class CurrentUserDto(
    val discordUserId: String,
    val displayName: String,
    val username: String,
    val avatarUrl: String?
) {
    /**
     * Maps this value to domain.
     */
    fun toDomain(): CurrentUser = CurrentUser(
        id = discordUserId,
        displayName = displayName,
        username = username,
        avatarUrl = avatarUrl
    )
}
