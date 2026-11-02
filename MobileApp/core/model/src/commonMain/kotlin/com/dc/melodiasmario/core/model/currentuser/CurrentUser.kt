package com.dc.melodiasmario.core.model.currentuser

/**
 * Represents current user shared between application layers.
 *
 * @property id Id.
 * @property displayName Display name.
 * @property username Username.
 * @property avatarUrl Avatar url.
 */
data class CurrentUser(
    val id: String,
    val displayName: String,
    val username: String,
    val avatarUrl: String?
)
