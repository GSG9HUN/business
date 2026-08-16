package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.Composable
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.remember
import androidx.navigation3.runtime.entryProvider
import androidx.navigation3.ui.NavDisplay
import com.dc.melodiasmario.shared.ui.screen.addsong.AddSongRoute
import com.dc.melodiasmario.shared.ui.screen.auth.LoginRoute
import com.dc.melodiasmario.shared.ui.screen.currentmusic.CurrentMusicRoute
import com.dc.melodiasmario.shared.ui.screen.guild.GuildSelectorRoute
import com.dc.melodiasmario.shared.ui.screen.playlists.PlaylistSongsRoute
import com.dc.melodiasmario.shared.ui.screen.playlists.PlaylistsRoute
import com.dc.melodiasmario.shared.ui.screen.profile.ProfileRoute
import com.dc.melodiasmario.shared.ui.screen.queue.QueueRoute
import com.dc.melodiasmario.shared.ui.screen.removesong.RemoveSongRoute
import com.dc.melodiasmario.shared.ui.screen.settings.SettingsRoute

@Composable
fun Navigation() {
    val backStack = remember { mutableStateListOf<AppRoute>(AppRoute.Login) }

    NavDisplay(
        backStack = backStack,
        onBack = {
            backStack.takeIf { it.size > 1 }?.let {
               backStack.removeLastOrNull()
            }
        },
        entryProvider = entryProvider {
            entry<AppRoute.Login> {
                LoginRoute(
                    onLoginSuccess = {
                        backStack.clear()
                        backStack.add(AppRoute.GuildSelector)
                    }
                )
            }

            entry<AppRoute.GuildSelector> {
                GuildSelectorRoute(
                    onAvatarClicked = { profileId ->
                        backStack.add(AppRoute.Profile(profileId))
                    },
                    onGuildClicked = { guildId ->
                        backStack.add(AppRoute.Playlists(guildId))
                    }
                )
            }

            entry<AppRoute.Playlists> { route ->
                PlaylistsRoute(guildId = route.guildId)
            }

            entry<AppRoute.Profile> { route ->
                ProfileRoute(profileId = route.profileId)
            }

            entry<AppRoute.PlaylistSongs> {
                PlaylistSongsRoute()
            }

            entry<AppRoute.Queue> {
                QueueRoute()
            }

            entry<AppRoute.CurrentMusic> {
                CurrentMusicRoute()
            }

            entry<AppRoute.AddSong> {
                AddSongRoute()
            }

            entry<AppRoute.RemoveSong> {
                RemoveSongRoute()
            }

            entry<AppRoute.Settings> {
                SettingsRoute()
            }
        }
    )
}