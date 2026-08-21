package com.dc.melodiasmario.shared.ui.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.navigation3.runtime.entryProvider
import com.dc.melodiasmario.feature.addsong.ui.AddSongRoute
import com.dc.melodiasmario.feature.login.ui.LoginRoute
import com.dc.melodiasmario.feature.currentmusic.ui.CurrentMusicRoute
import com.dc.melodiasmario.feature.guild.ui.GuildSelectorRoute
import com.dc.melodiasmario.feature.playlists.ui.PlaylistSongsRoute
import com.dc.melodiasmario.feature.playlists.ui.PlaylistsRoute
import com.dc.melodiasmario.feature.profile.ui.ProfileRoute
import com.dc.melodiasmario.feature.queue.ui.QueueRoute
import com.dc.melodiasmario.feature.removesong.ui.RemoveSongRoute
import com.dc.melodiasmario.feature.settings.ui.SettingsRoute

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

    entry<AppRoute.PlaylistSongs> { route ->
        PlaylistSongsRoute(playlistId = route.playlistId)
    }

    entry<AppRoute.Queue> { route ->
        QueueRoute(guildId = route.guildId)
    }

    entry<AppRoute.CurrentMusic> { route ->
        CurrentMusicRoute(guildId = route.guildId)
    }

    entry<AppRoute.AddSong> { route ->
        AddSongRoute(guildId = route.guildId)
    }

    entry<AppRoute.RemoveSong> { route ->
        RemoveSongRoute(guildId = route.guildId)
    }

    entry<AppRoute.Settings> { route ->
        SettingsRoute(guildId = route.guildId)
    }
}