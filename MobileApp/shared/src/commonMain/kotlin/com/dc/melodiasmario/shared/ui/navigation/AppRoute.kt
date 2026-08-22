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
    data object MyProfile : AppRoute

    @Serializable
    data class Playlists(val guildId: String) : AppRoute
    @Serializable
    data class Queue(val guildId: String) : AppRoute
    @Serializable
    data class CurrentMusic(val guildId: String) : AppRoute
    @Serializable
    data class Settings(val guildId: String) : AppRoute
    @Serializable
    data class AddSong(val guildId: String) : AppRoute
    @Serializable
    data class RemoveSong(val guildId: String) : AppRoute
    @Serializable
    data class PlaylistSongs(
        val guildId: String,
        val playlistId: String
    ) : AppRoute
}

fun AppRoute.guildIdOrNull(): String? {
    return when (this) {
        is AppRoute.Playlists -> guildId
        is AppRoute.Queue -> guildId
        is AppRoute.CurrentMusic -> guildId
        is AppRoute.Settings -> guildId
        is AppRoute.AddSong -> guildId
        is AppRoute.RemoveSong -> guildId
        is AppRoute.PlaylistSongs -> guildId
        else -> null
    }
}

fun AppRoute.shouldShowBottomBar(): Boolean {
    return guildIdOrNull() != null
}

fun AppRoute.isMainTabRoute(): Boolean {
    return this is AppRoute.Playlists ||
            this is AppRoute.Queue ||
            this is AppRoute.CurrentMusic ||
            this is AppRoute.Settings
}