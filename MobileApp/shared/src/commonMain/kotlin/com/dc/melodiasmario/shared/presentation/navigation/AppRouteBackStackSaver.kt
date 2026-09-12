package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.saveable.Saver
import androidx.compose.runtime.snapshots.SnapshotStateList

val AppRouteBackStackSaver = Saver<SnapshotStateList<AppRoute>, List<List<String>>>(
    save = { backStack ->
        backStack.map { route ->
            when (route) {
                AppRoute.Login -> listOf("Login")
                AppRoute.GuildSelector -> listOf("GuildSelector")
                AppRoute.MyProfile -> listOf("MyProfile")
                is AppRoute.Playlists -> listOf("Playlists", route.guildId)
                is AppRoute.PlaylistSongs -> listOf("PlaylistSongs", route.guildId, route.playlistId)
                is AppRoute.Queue -> listOf("Queue", route.guildId)
                is AppRoute.CurrentMusic -> listOf("CurrentMusic", route.guildId)
                is AppRoute.PlaylistSong -> listOf("PlaylistSong", route.guildId)
                is AppRoute.RemoveSong -> listOf("RemoveSong", route.guildId)
                is AppRoute.Settings -> listOf("Settings", route.guildId)
            }
        }
    },
    restore = { saved ->
        mutableStateListOf<AppRoute>().apply {
            addAll(saved.mapNotNull { route ->
                when (route.firstOrNull()) {
                    "Login" -> AppRoute.Login
                    "GuildSelector" -> AppRoute.GuildSelector
                    "MyProfile" -> AppRoute.MyProfile
                    "Playlists" -> route.getOrNull(1)?.let(AppRoute::Playlists)
                    "PlaylistSongs" -> route.getOrNull(1)?.let { guildId ->
                        route.getOrNull(2)?.let { playlistId ->
                            AppRoute.PlaylistSongs(guildId = guildId, playlistId = playlistId)
                        }
                    }
                    "Queue" -> route.getOrNull(1)?.let(AppRoute::Queue)
                    "CurrentMusic" -> route.getOrNull(1)?.let(AppRoute::CurrentMusic)
                    "PlaylistSong" -> route.getOrNull(1)?.let(AppRoute::PlaylistSong)
                    "RemoveSong" -> route.getOrNull(1)?.let(AppRoute::RemoveSong)
                    "Settings" -> route.getOrNull(1)?.let(AppRoute::Settings)
                    else -> null
                }
            }.ifEmpty { listOf(AppRoute.Login) })
        }
    },
)
