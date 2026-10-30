package com.dc.melodiasmario.feature.guild.presentation

import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import com.dc.melodiasmario.core.model.guild.Guild

/**
 * Defines guild selector presentation state shared with UI consumers.
 *
 * @property currentUser Current user.
 * @property guilds Guilds.
 * @property isLoading Is loading.
 * @property errorMessage Error message.
 * @property searchQuery Search query.
 * @property filteredGuilds Filtered guilds.
 */
data class GuildSelectorUiState(
    val currentUser: CurrentUser = CurrentUser(
        id = "",
        displayName = "",
        username = "",
        avatarUrl = null
    ),
    val guilds: List<Guild> = emptyList(),
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val searchQuery: String = "",
    val filteredGuilds: List<Guild> = emptyList()
)
