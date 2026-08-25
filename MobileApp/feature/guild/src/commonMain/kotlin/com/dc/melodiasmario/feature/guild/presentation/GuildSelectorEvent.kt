package com.dc.melodiasmario.feature.guild.presentation

sealed interface GuildSelectorEvent {
    data object GetGuilds : GuildSelectorEvent
    data object GetCurrentUser : GuildSelectorEvent
    data object RefreshClicked : GuildSelectorEvent
    data object AvatarClicked : GuildSelectorEvent
    data class SearchQueryChanged(val query: String) : GuildSelectorEvent
    data class GuildClicked(val guildId: String) : GuildSelectorEvent
}
