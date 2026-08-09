package com.dc.melodiasmario.shared.presentation.guild

sealed interface GuildSelectorEvent {
    data object GetGuilds : GuildSelectorEvent
    data object RefreshClicked : GuildSelectorEvent
    data object AvatarClicked : GuildSelectorEvent
    data class SearchQueryChanged(val query: String) : GuildSelectorEvent
    data class GuildClicked(val guildId: String) : GuildSelectorEvent
}
