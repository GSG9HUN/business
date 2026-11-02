package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.saveable.Saver
import androidx.compose.runtime.snapshots.SnapshotStateList
import com.dc.melodiasmario.core.common.navigation.AppRoute

/**
 * App route back stack saver exposed by this module.
 */
val AppRouteBackStackSaver: Saver<SnapshotStateList<AppRoute>, List<List<String>>> =
    Saver<SnapshotStateList<AppRoute>, List<List<String>>>(
    save = { backStack ->
        backStack.map { route ->
            when (route) {
                AppRoute.Login -> listOf("Login")
                AppRoute.GuildSelector -> listOf("GuildSelector")
                AppRoute.MyProfile -> listOf("MyProfile")
                is AppRoute.Playlists -> listOf("Playlists", route.guildId)
                is AppRoute.Queue -> listOf("Queue", route.guildId)
                is AppRoute.CurrentTrack -> listOf("CurrentTrack", route.guildId)
                is AppRoute.PlaylistTrack -> listOf(
                    "PlaylistTrack",
                    route.guildId,
                    route.playlistId
                )

                is AppRoute.RemoveSong -> listOf("RemoveSong", route.guildId)
                is AppRoute.Settings -> listOf("Settings", route.guildId)
            }
        }
    },
    restore = { saved ->
        mutableStateListOf<AppRoute>().apply {
            addAll(
                saved.mapNotNull { route ->
                    when (route.firstOrNull()) {
                        "Login" -> AppRoute.Login
                        "GuildSelector" -> AppRoute.GuildSelector
                        "MyProfile" -> AppRoute.MyProfile
                        "Playlists" -> route.getOrNull(index = 1)
                            ?.let(block = AppRoute::Playlists)
                        "Queue" -> route.getOrNull(index = 1)
                            ?.let(block = AppRoute::Queue)
                        "CurrentTrack" -> route.getOrNull(index = 1)
                            ?.let(block = AppRoute::CurrentTrack)
                        "PlaylistTrack" -> route.getOrNull(index = 1)
                            ?.let { guildId ->
                                route.getOrNull(index = 2)
                                    ?.let { playlistId ->
                                        AppRoute.PlaylistTrack(guildId = guildId, playlistId = playlistId)
                                    } ?: AppRoute.Playlists(guildId = guildId)
                            }

                        "RemoveSong" -> route.getOrNull(index = 1)
                            ?.let(block = AppRoute::RemoveSong)
                        "Settings" -> route.getOrNull(index = 1)
                            ?.let(block = AppRoute::Settings)
                        else -> null
                    }
                }
                    .ifEmpty { listOf(AppRoute.Login) }
            )
        }
    }
)
