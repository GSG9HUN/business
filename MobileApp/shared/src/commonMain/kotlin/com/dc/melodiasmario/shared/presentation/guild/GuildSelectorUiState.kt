package com.dc.melodiasmario.shared.presentation.guild

import com.dc.melodiasmario.shared.domain.guild.model.Guild

data class GuildSelectorUiState(
    val profileId: String,
    val guilds: List<Guild> = emptyList(),
    val isLoading: Boolean = false,
    val errorMessage: String? = null,
    val searchQuery: String = "",
    val filteredGuilds: List<Guild> = emptyList(),
)
