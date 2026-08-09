package com.dc.melodiasmario.shared.presentation.guild

sealed interface GuildSelectorEffect {
    data class NavigateToPlaylists(val guildId: String) : GuildSelectorEffect
    data class NavigateToProfile(val profileId: String) : GuildSelectorEffect
}
