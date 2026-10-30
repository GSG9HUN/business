package com.dc.melodiasmario.core.common.navigation

import androidx.navigation3.runtime.NavKey
import kotlinx.serialization.Serializable

/**
 * Provides app route for the navigation layer.
 */
@Serializable
sealed interface AppRoute : NavKey {

    /**
     * Groups login values used by app route.
     */
    @Serializable
    data object Login : AppRoute

    /**
     * Groups guild selector values used by app route.
     */
    @Serializable
    data object GuildSelector : AppRoute

    /**
     * Groups my profile values used by app route.
     */
    @Serializable
    data object MyProfile : AppRoute

    /**
     * Provides playlists for the navigation layer.
     *
     * @property guildId Identifier of the guild owning this value.
     */
    @Serializable
    data class Playlists(val guildId: String) : AppRoute

    /**
     * Provides queue for the navigation layer.
     *
     * @property guildId Identifier of the guild owning this value.
     */
    @Serializable
    data class Queue(val guildId: String) : AppRoute

    /**
     * Provides current track for the navigation layer.
     *
     * @property guildId Identifier of the guild owning this value.
     */
    @Serializable
    data class CurrentTrack(val guildId: String) : AppRoute

    /**
     * Provides settings for the navigation layer.
     *
     * @property guildId Identifier of the guild owning this value.
     */
    @Serializable
    data class Settings(val guildId: String) : AppRoute

    /**
     * Provides playlist track for the navigation layer.
     *
     * @property guildId Identifier of the guild owning this value.
     * @property playlistId Identifier of the saved playlist.
     */
    @Serializable
    data class PlaylistTrack(val guildId: String, val playlistId: String) : AppRoute

    /**
     * Provides remove song for the navigation layer.
     *
     * @property guildId Identifier of the guild owning this value.
     */
    @Serializable
    data class RemoveSong(val guildId: String) : AppRoute
}

/**
 * Provides the guild id or null operation for app route.
 */
fun AppRoute.guildIdOrNull(): String? {
    return when (this) {
        is AppRoute.Playlists -> guildId
        is AppRoute.Queue -> guildId
        is AppRoute.CurrentTrack -> guildId
        is AppRoute.Settings -> guildId
        is AppRoute.PlaylistTrack -> guildId
        is AppRoute.RemoveSong -> guildId
        else -> null
    }
}

/**
 * Provides the should show bottom bar operation for app route.
 */
fun AppRoute.shouldShowBottomBar(): Boolean {
    return guildIdOrNull() != null && this !is AppRoute.PlaylistTrack
}

/**
 * Provides the should show floating button operation for app route.
 */
fun AppRoute.shouldShowFloatingButton(): Boolean {
    return when (this) {
        is AppRoute.Playlists -> true
        is AppRoute.Queue -> true
        is AppRoute.CurrentTrack -> true
        else -> false
    }
}

/**
 * Reports whether main tab route.
 */
fun AppRoute.isMainTabRoute(): Boolean {
    return this is AppRoute.Playlists ||
            this is AppRoute.Queue ||
            this is AppRoute.CurrentTrack ||
            this is AppRoute.Settings
}
