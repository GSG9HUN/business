package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.navigation.NavGraphBuilder
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.rememberNavController
import androidx.savedstate.read
import com.dc.melodiasmario.shared.ui.screen.addsong.AddSongRoute
import com.dc.melodiasmario.shared.ui.screen.auth.LoginRoute
import com.dc.melodiasmario.shared.ui.screen.currentmusic.CurrentMusicRoute
import com.dc.melodiasmario.shared.ui.screen.playlists.PlaylistSongsRoute
import com.dc.melodiasmario.shared.ui.screen.playlists.PlaylistsRoute
import com.dc.melodiasmario.shared.ui.screen.profile.ProfileRoute
import com.dc.melodiasmario.shared.ui.screen.queue.QueueRoute
import com.dc.melodiasmario.shared.ui.screen.removesong.RemoveSongRoute
import com.dc.melodiasmario.shared.ui.screen.guild.GuildSelectorRoute
import com.dc.melodiasmario.shared.ui.screen.settings.SettingsRoute

@Composable
fun Navigation() {
    val navController = rememberNavController()

    NavHost(
        navController = navController,
        startDestination = AppRoute.Login,
    ) {
        loginRoute(navController = navController)
        guildSelectorRoute(navController = navController)
        playlistsRoute(navController = navController)
        playlistSongsRoute(navController = navController)
        queueRoute(navController = navController)
        currentMusicRoute(navController = navController)
        addSongRoute(navController = navController)
        removeSongRoute(navController = navController)
        settingsRoute(navController = navController)
        profileRoute(navController = navController)
    }
}

private fun NavGraphBuilder.loginRoute(navController: NavHostController) {
    composable(AppRoute.Login) {
        LoginRoute(
            onLoginSuccess = {
                navController.navigate(AppRoute.GuildSelector) {
                    popUpTo(AppRoute.Login) {
                        inclusive = true
                    }
                    launchSingleTop = true
                }
            })
    }
}

private fun NavGraphBuilder.guildSelectorRoute(navController: NavHostController) {
    composable(AppRoute.GuildSelector) {
        GuildSelectorRoute(onAvatarClicked = { profileId ->
            navController.navigate(
                AppRoute.profile(
                    profileId = profileId
                )
            )
        }, onGuildClicked = { guildId ->
            navController.navigate(
                AppRoute.playlists(
                    guildId = guildId
                )
            )
        })
    }
}

private fun NavGraphBuilder.playlistsRoute(navController: NavHostController) {
    composable(AppRoute.Playlists) { navBackStackEntry ->
        val guildId = navBackStackEntry.arguments?.read {
            getStringOrNull("guildId")
        } ?: return@composable
        PlaylistsRoute(
            guildId = guildId
        )
    }
}

private fun NavGraphBuilder.profileRoute(navController: NavHostController) {
    composable(AppRoute.Profile) { navBackStackEntry ->
        val profileId = navBackStackEntry.arguments?.read {
            getStringOrNull("profileId")
        } ?: return@composable
        ProfileRoute(
            profileId = profileId
        )
    }
}

private fun NavGraphBuilder.playlistSongsRoute(navController: NavHostController) {
    composable(AppRoute.PlaylistSongs) {
        PlaylistSongsRoute()
    }
}

private fun NavGraphBuilder.queueRoute(navController: NavHostController) {
    composable(AppRoute.Queue) {
        QueueRoute()
    }
}

private fun NavGraphBuilder.currentMusicRoute(navController: NavHostController) {
    composable(AppRoute.CurrentMusic) {
        CurrentMusicRoute()
    }
}

private fun NavGraphBuilder.addSongRoute(navController: NavHostController) {
    composable(AppRoute.AddSong) {
        AddSongRoute()
    }
}

private fun NavGraphBuilder.removeSongRoute(navController: NavHostController) {
    composable(AppRoute.RemoveSong) {
        RemoveSongRoute()
    }
}

private fun NavGraphBuilder.settingsRoute(navController: NavHostController) {
    composable(AppRoute.Settings) {
        SettingsRoute()
    }
}


