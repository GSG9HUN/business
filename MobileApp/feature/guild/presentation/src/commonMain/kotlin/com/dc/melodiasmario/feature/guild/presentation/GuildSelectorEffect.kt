package com.dc.melodiasmario.feature.guild.presentation

import com.dc.melodiasmario.core.presentation.MviEffect

/**
 * Defines one-time guild selector presentation outcomes.
 */
sealed interface GuildSelectorEffect : MviEffect {
    /**
     * Describes the navigate to playlists variant of [GuildSelectorEffect].
     *
     * @property guildId Identifier of the guild owning this value.
     */
    data class NavigateToPlaylists(val guildId: String) : GuildSelectorEffect

    /**
     * Describes the navigate to profile variant of [GuildSelectorEffect].
     */
    data object NavigateToProfile : GuildSelectorEffect
}
