package com.dc.melodiasmario.feature.profile.domain.model

data class ProfileUser(
    val id: String,
    val displayName: String,
    val username: String,
    val provider: String,
    val avatarUrl: String?,
    val isActive: Boolean,
    val isDiscordConnected: Boolean,
) {
    companion object {
        val Default = ProfileUser(
            id = "",
            displayName = "",
            username = "",
            provider = "",
            avatarUrl = null,
            isActive = false,
            isDiscordConnected = false,
        )
    }
}
