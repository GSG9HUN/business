package com.dc.melodiasmario.feature.guild.presentation

sealed interface GuildSelectorEffect {
    data class NavigateToPlaylists(val guildId: String) : GuildSelectorEffect
    data class NavigateToProfile(val profileId: String) : GuildSelectorEffect
}
