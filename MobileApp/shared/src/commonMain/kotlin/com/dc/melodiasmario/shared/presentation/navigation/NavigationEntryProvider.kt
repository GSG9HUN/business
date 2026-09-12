package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.navigation3.runtime.entryProvider
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.currentmusic.presentation.navigation.CurrentMusicEntryProvider
import com.dc.melodiasmario.feature.guild.presentation.navigation.GuildEntryProvider
import com.dc.melodiasmario.feature.login.presentation.navigation.LoginEntryProvider
import com.dc.melodiasmario.feature.playlist.presentation.navigation.PlaylistEntryProvider
import com.dc.melodiasmario.feature.playlistsong.presentation.navigation.PlaylistSongEntryProvider
import com.dc.melodiasmario.feature.profile.presentation.navigation.ProfileEntryProvider
import com.dc.melodiasmario.feature.queue.presentation.navigation.QueueEntryProvider
import com.dc.melodiasmario.feature.removesong.presentation.navigation.RemoveSongEntryProvider
import com.dc.melodiasmario.feature.settings.presentation.navigation.SettingsEntryProvider

private val loginEntryProvider = LoginEntryProvider()
private val guildEntryProvider = GuildEntryProvider()
private val playlistEntryProvider = PlaylistEntryProvider()
private val profileEntryProvider = ProfileEntryProvider()
private val currentMusicEntryProvider = CurrentMusicEntryProvider()
private val queueEntryProvider = QueueEntryProvider()
private val playlistSongEntryProvider = PlaylistSongEntryProvider()
private val removeSongEntryProvider = RemoveSongEntryProvider()
private val settingsEntryProvider = SettingsEntryProvider()

fun navigationEntryProvider(
    backStack: SnapshotStateList<AppRoute>,
    toastHostState: MToastHostState,
) = entryProvider {
    entry<AppRoute.Login> {
        loginEntryProvider.Entry(
            onLoginSuccess = {
                backStack.replaceAll(AppRoute.GuildSelector)
            },
        )
    }

    entry<AppRoute.GuildSelector> {
        guildEntryProvider.Entry(
            onAvatarClicked = {
                backStack.navigate(AppRoute.MyProfile)
            },
            onGuildClicked = { guildId ->
                backStack.navigate(AppRoute.Playlists(guildId))
            },
        )
    }

    entry<AppRoute.Playlists> { route ->
        playlistEntryProvider.PlaylistsEntry(
            guildId = route.guildId,
            onPlaylistClicked = { playlistId ->
                backStack.navigate(
                    AppRoute.PlaylistSongs(
                        guildId = route.guildId,
                        playlistId = playlistId,
                    )
                )
            },
        )
    }

    entry<AppRoute.MyProfile> {
        profileEntryProvider.Entry(
            onBack = {
                backStack.goBack()
            },
            logout = {
                backStack.replaceAll(AppRoute.Login)
            },
            toastHostState = toastHostState,
        )
    }

    entry<AppRoute.PlaylistSongs> { route ->
        playlistEntryProvider.PlaylistSongsEntry(
            guildId = route.guildId,
            playlistId = route.playlistId,
        )
    }

    entry<AppRoute.Queue> { route ->
        queueEntryProvider.Entry(guildId = route.guildId)
    }

    entry<AppRoute.CurrentMusic> { route ->
        currentMusicEntryProvider.Entry(guildId = route.guildId)
    }

    entry<AppRoute.PlaylistSong> { route ->
        playlistSongEntryProvider.Entry(guildId = route.guildId)
    }

    entry<AppRoute.RemoveSong> { route ->
        removeSongEntryProvider.Entry(guildId = route.guildId)
    }

    entry<AppRoute.Settings> { route ->
        settingsEntryProvider.Entry(guildId = route.guildId)
    }
}
