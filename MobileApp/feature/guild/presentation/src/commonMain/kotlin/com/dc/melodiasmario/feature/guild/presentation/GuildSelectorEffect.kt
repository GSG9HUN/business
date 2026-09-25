package com.dc.melodiasmario.feature.guild.presentation

import com.dc.melodiasmario.core.common.presentation.MviEffect

sealed interface GuildSelectorEffect : MviEffect {
    data class NavigateToPlaylists(val guildId: String) : GuildSelectorEffect
    data object NavigateToProfile : GuildSelectorEffect
}
