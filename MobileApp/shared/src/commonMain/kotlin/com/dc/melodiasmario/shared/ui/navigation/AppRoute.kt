package com.dc.melodiasmario.shared.ui.navigation

import androidx.navigation3.runtime.NavKey
import kotlinx.serialization.Serializable

@Serializable
sealed interface AppRoute : NavKey {
    @Serializable
    data object Login : AppRoute

    @Serializable
    data object GuildSelector : AppRoute

    @Serializable
    data class Profile(val profileId: String) : AppRoute

    @Serializable
    data class Playlists(val guildId: String) : AppRoute

    @Serializable
    data object PlaylistSongs : AppRoute
    @Serializable
    data object Queue : AppRoute
    @Serializable
    data object CurrentMusic : AppRoute
    @Serializable
    data object AddSong : AppRoute
    @Serializable
    data object RemoveSong : AppRoute
    @Serializable
    data object Settings : AppRoute
}