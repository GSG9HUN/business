package com.dc.melodiasmario.shared.presentation.navigation

import androidx.compose.runtime.snapshots.SnapshotStateList
import androidx.navigation3.runtime.NavEntry
import androidx.navigation3.runtime.entryProvider
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.core.commonui.feedback.state.MToastHostState
import com.dc.melodiasmario.feature.currenttrack.presentation.navigation.currentTrackEntry
import com.dc.melodiasmario.feature.guild.presentation.navigation.guildSelectorEntry
import com.dc.melodiasmario.feature.login.presentation.navigation.loginEntry
import com.dc.melodiasmario.feature.playlist.presentation.navigation.playlistsEntry
import com.dc.melodiasmario.feature.playlisttrack.presentation.navigation.playlistTrackEntry
import com.dc.melodiasmario.feature.profile.presentation.navigation.profileEntry
import com.dc.melodiasmario.feature.queue.presentation.navigation.queueEntry
import com.dc.melodiasmario.feature.removesong.presentation.navigation.removeSongEntry
import com.dc.melodiasmario.feature.settings.presentation.navigation.settingsEntry

/**
 * Builds the route-to-entry mapping and connects destination callbacks to the shared back stack.
 */
fun navigationEntryProvider(
    backStack: SnapshotStateList<AppRoute>,
    toastHostState: MToastHostState
): (route: AppRoute) -> NavEntry<AppRoute> = entryProvider {
    loginEntry(
        onLoginSuccess = {
            backStack.replaceAll(route = AppRoute.GuildSelector)
        }
    )

    guildSelectorEntry(
        onAvatarClicked = {
            backStack.navigate(route = AppRoute.MyProfile)
        },
        onGuildClicked = { guildId ->
            backStack.navigate(route = AppRoute.Playlists(guildId = guildId))
        }
    )

    playlistsEntry(
        toastHostState = toastHostState,
        onPlaylistClicked = { route, playlistId ->
            backStack.navigate(
                route = AppRoute.PlaylistTrack(
                    guildId = route.guildId,
                    playlistId = playlistId
                )
            )
        }
    )

    profileEntry(
        onBack = {
            backStack.goBack()
        },
        logout = {
            backStack.replaceAll(route = AppRoute.Login)
        },
        toastHostState = toastHostState
    )

    queueEntry(
        onGuildClicked = { backStack.replaceAll(route = AppRoute.GuildSelector) },
        toastHostState = toastHostState
    )

    currentTrackEntry(
        onGuildClicked = {
            backStack.replaceAll(route = AppRoute.GuildSelector)
        },
        onProfileClicked = {
            backStack.navigate(route = AppRoute.MyProfile)
        },
        toastHostState = toastHostState
    )

    playlistTrackEntry(
        onBack = { backStack.goBack() },
        toastHostState = toastHostState
    )

    removeSongEntry()

    settingsEntry()
}
