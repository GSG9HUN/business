package com.dc.melodiasmario.feature.guild.presentation.navigation

import androidx.navigation3.runtime.EntryProviderScope
import com.dc.melodiasmario.core.common.navigation.AppRoute
import com.dc.melodiasmario.feature.guild.ui.GuildSelectorRoute

/**
 * Registers the guild selector destination and its navigation callbacks.
 */
fun EntryProviderScope<AppRoute>.guildSelectorEntry(
    onAvatarClicked: () -> Unit,
    onGuildClicked: (guildId: String) -> Unit
): Unit {
    entry<AppRoute.GuildSelector> {
        GuildSelectorRoute(
            onAvatarClicked = onAvatarClicked,
            onGuildClicked = onGuildClicked
        )
    }
}
