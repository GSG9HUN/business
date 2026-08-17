package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.navigation3.runtime.entryProvider
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

fun navigationEntryProvider(
    backStack: SnapshotStateList<AppRoute>,
) = entryProvider {
    entry<AppRoute.Login> {
        LoginRoute(
            onLoginSuccess = {
                backStack.replaceAll(AppRoute.GuildSelector)
            },
        )
    }

    entry<AppRoute.GuildSelector> {
        GuildSelectorRoute(
            onAvatarClicked = { profileId ->
                backStack.navigate(AppRoute.Profile(profileId))
            },
            onGuildClicked = { guildId ->
                backStack.navigate(AppRoute.Playlists(guildId))
            },
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