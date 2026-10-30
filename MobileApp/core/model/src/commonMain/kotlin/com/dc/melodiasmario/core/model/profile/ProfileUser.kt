package com.dc.melodiasmario.core.model.profile

/**
 * Represents profile user shared between application layers.
 *
 * @property id Id.
 * @property displayName Display name.
 * @property username Username.
 * @property provider Provider.
 * @property avatarUrl Avatar url.
 * @property isActive Is active.
 * @property isDiscordConnected Is discord connected.
 */
data class ProfileUser(
    val id: String,
    val displayName: String,
    val username: String,
    val provider: String,
    val avatarUrl: String?,
    val isActive: Boolean,
    val isDiscordConnected: Boolean
) {
    /**
     * Shared values and operations for the enclosing type.
     */
    companion object {
        /**
         * Default exposed by companion.
         */
        val Default: ProfileUser = ProfileUser(
            id = "",
            displayName = "",
            username = "",
            provider = "",
            avatarUrl = null,
            isActive = false,
            isDiscordConnected = false
        )
    }
}
