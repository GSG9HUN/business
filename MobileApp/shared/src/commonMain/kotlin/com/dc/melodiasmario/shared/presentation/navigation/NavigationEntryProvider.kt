package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.navigation3.runtime.entryProvider
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.currentmusic.presentation.navigation.currentMusicEntry
import com.dc.melodiasmario.feature.guild.presentation.navigation.guildSelectorEntry
import com.dc.melodiasmario.feature.login.presentation.navigation.loginEntry
import com.dc.melodiasmario.feature.playlist.presentation.navigation.playlistSongsEntry
import com.dc.melodiasmario.feature.playlist.presentation.navigation.playlistsEntry
import com.dc.melodiasmario.feature.playlistsong.presentation.navigation.playlistSongEntry
import com.dc.melodiasmario.feature.profile.presentation.navigation.profileEntry
import com.dc.melodiasmario.feature.queue.presentation.navigation.queueEntry
import com.dc.melodiasmario.feature.removesong.presentation.navigation.removeSongEntry
import com.dc.melodiasmario.feature.settings.presentation.navigation.settingsEntry

fun navigationEntryProvider(
    backStack: SnapshotStateList<AppRoute>,
    toastHostState: MToastHostState,
) = entryProvider {
    loginEntry(
        onLoginSuccess = {
            backStack.replaceAll(AppRoute.GuildSelector)
        },
    )

    guildSelectorEntry(
        onAvatarClicked = {
            backStack.navigate(AppRoute.MyProfile)
        },
        onGuildClicked = { guildId ->
            backStack.navigate(AppRoute.Playlists(guildId))
        },
    )

    playlistsEntry(
        toastHostState = toastHostState,
        onPlaylistClicked = { route, playlistId ->
            backStack.navigate(
                AppRoute.PlaylistSongs(
                    guildId = route.guildId,
                    playlistId = playlistId,
                )
            )
        },
    )

    profileEntry(
        onBack = {
            backStack.goBack()
        },
        logout = {
            backStack.replaceAll(AppRoute.Login)
        },
        toastHostState = toastHostState,
    )

    playlistSongsEntry()

    queueEntry()

    currentMusicEntry()

    playlistSongEntry()

    removeSongEntry()

    settingsEntry()
}
