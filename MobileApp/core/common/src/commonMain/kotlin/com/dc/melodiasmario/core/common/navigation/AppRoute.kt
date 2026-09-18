package com.dc.melodiasmario.core.common.navigation

import androidx.navigation3.runtime.NavKey
import kotlinx.serialization.Serializable

@Serializable
sealed interface AppRoute : NavKey {

    @Serializable
    data object Login : AppRoute

    @Serializable
    data object GuildSelector : AppRoute

    @Serializable
    data object MyProfile : AppRoute

    @Serializable
    data class Playlists(val guildId: String) : AppRoute

    @Serializable
    data class Queue(val guildId: String) : AppRoute

    @Serializable
    data class CurrentTrack(val guildId: String) : AppRoute

    @Serializable
    data class Settings(val guildId: String) : AppRoute

    @Serializable
    data class PlaylistSong(val guildId: String) : AppRoute

    @Serializable
    data class RemoveSong(val guildId: String) : AppRoute

    @Serializable
    data class PlaylistSongs(
        val guildId: String,
        val playlistId: String,
    ) : AppRoute
}

fun AppRoute.guildIdOrNull(): String? {
    return when (this) {
        is AppRoute.Playlists -> guildId
        is AppRoute.Queue -> guildId
        is AppRoute.CurrentTrack -> guildId
        is AppRoute.Settings -> guildId
        is AppRoute.PlaylistSong -> guildId
        is AppRoute.RemoveSong -> guildId
        is AppRoute.PlaylistSongs -> guildId
        else -> null
    }
}

fun AppRoute.shouldShowBottomBar(): Boolean {
    return guildIdOrNull() != null
}

fun AppRoute.shouldShowFloatingButton(): Boolean {
    return when (this) {
        is AppRoute.Playlists -> true
        is AppRoute.Queue -> true
        is AppRoute.CurrentTrack -> true
        else -> false
    }
}

fun AppRoute.isMainTabRoute(): Boolean {
    return this is AppRoute.Playlists ||
            this is AppRoute.Queue ||
            this is AppRoute.CurrentTrack ||
            this is AppRoute.Settings
}
