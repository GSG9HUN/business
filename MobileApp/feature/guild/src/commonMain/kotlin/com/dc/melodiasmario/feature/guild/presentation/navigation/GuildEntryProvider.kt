package com.dc.melodiasmario.feature.guild.presentation.navigation

import androidx.compose.runtime.Composable
import com.dc.melodiasmario.feature.guild.ui.GuildSelectorRoute

class GuildEntryProvider {
    @Composable
    fun Entry(
        onAvatarClicked: () -> Unit,
        onGuildClicked: (guildId: String) -> Unit,
    ) {
        GuildSelectorRoute(
            onAvatarClicked = onAvatarClicked,
            onGuildClicked = onGuildClicked,
        )
    }
}
