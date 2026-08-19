package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.saveable.Saver
import androidx.compose.runtime.snapshots.SnapshotStateList

val AppRouteBackStackSaver = Saver<SnapshotStateList<AppRoute>, List<List<String>>>(
    save = { backStack ->
        backStack.map { route ->
            when (route) {
                AppRoute.Login -> listOf("Login")
                AppRoute.GuildSelector -> listOf("GuildSelector")
                is AppRoute.Profile -> listOf("Profile", route.profileId)
                is AppRoute.Playlists -> listOf("Playlists", route.guildId)
                AppRoute.PlaylistSongs -> listOf("PlaylistSongs")
                AppRoute.Queue -> listOf("Queue")
                AppRoute.CurrentMusic -> listOf("CurrentMusic")
                AppRoute.AddSong -> listOf("AddSong")
                AppRoute.RemoveSong -> listOf("RemoveSong")
                AppRoute.Settings -> listOf("Settings")
            }
        }
    },
    restore = { saved ->
        mutableStateListOf<AppRoute>().apply {
            addAll(saved.mapNotNull { route ->
                when (route.firstOrNull()) {
                    "Login" -> AppRoute.Login
                    "GuildSelector" -> AppRoute.GuildSelector
                    "Profile" -> route.getOrNull(1)?.let(AppRoute::Profile)
                    "Playlists" -> route.getOrNull(1)?.let(AppRoute::Playlists)
                    "PlaylistSongs" -> AppRoute.PlaylistSongs
                    "Queue" -> AppRoute.Queue
                    "CurrentMusic" -> AppRoute.CurrentMusic
                    "AddSong" -> AppRoute.AddSong
                    "RemoveSong" -> AppRoute.RemoveSong
                    "Settings" -> AppRoute.Settings
                    else -> null
                }
            }.ifEmpty { listOf(AppRoute.Login) })
        }
    },
)