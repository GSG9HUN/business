package com.dc.melodiasmario.feature.guild.presentation

/**
 * Defines the interactions handled by guild selector presentation logic.
 */
sealed interface GuildSelectorEvent {
    /**
     * Describes the get guilds variant of [GuildSelectorEvent].
     */
    data object GetGuilds : GuildSelectorEvent

    /**
     * Describes the get current user variant of [GuildSelectorEvent].
     */
    data object GetCurrentUser : GuildSelectorEvent

    /**
     * Describes the refresh clicked variant of [GuildSelectorEvent].
     */
    data object RefreshClicked : GuildSelectorEvent

    /**
     * Describes the avatar clicked variant of [GuildSelectorEvent].
     */
    data object AvatarClicked : GuildSelectorEvent

    /**
     * Describes the search query changed variant of [GuildSelectorEvent].
     *
     * @property query Query.
     */
    data class SearchQueryChanged(val query: String) : GuildSelectorEvent

    /**
     * Describes the guild clicked variant of [GuildSelectorEvent].
     *
     * @property guildId Identifier of the guild owning this value.
     */
    data class GuildClicked(val guildId: String) : GuildSelectorEvent
}
