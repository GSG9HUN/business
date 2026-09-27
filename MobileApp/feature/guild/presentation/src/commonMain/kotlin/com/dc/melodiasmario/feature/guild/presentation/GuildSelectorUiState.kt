package com.dc.melodiasmario.feature.guild.presentation

import com.dc.melodiasmario.core.model.currentuser.CurrentUser
import com.dc.melodiasmario.core.model.guild.Guild

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
    val filteredGuilds: List<Guild> = emptyList(),
)
