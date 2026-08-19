package com.dc.melodiasmario.feature.guild.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Scaffold
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.Avatar
import com.dc.melodiasmario.feature.guild.domain.model.Guild
import com.dc.melodiasmario.feature.guild.presentation.GuildSelectorEvent
import com.dc.melodiasmario.core.ui.components.MTopBar
import com.dc.melodiasmario.core.ui.components.RefreshButton
import com.dc.melodiasmario.core.ui.components.SearchBar
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MmBackground
import com.dc.melodiasmario.core.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.feature.guild.ui.components.GuildListItem
import com.dc.melodiasmario.core.ui.generated.resources.Res
import com.dc.melodiasmario.core.ui.generated.resources.available_guilds
import com.dc.melodiasmario.core.ui.generated.resources.guild_empty_message
import com.dc.melodiasmario.core.ui.generated.resources.guild_loading_message
import com.dc.melodiasmario.core.ui.generated.resources.guild_placeholder_title
import com.dc.melodiasmario.core.ui.generated.resources.search_placeholder
import com.dc.melodiasmario.core.ui.generated.resources.select_guild
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmTextPrimary
import com.dc.melodiasmario.feature.guild.domain.model.BotStatus
import com.dc.melodiasmario.feature.guild.domain.model.GuildAccessLevel
import com.dc.melodiasmario.feature.guild.ui.components.GuildSelectorPlaceholder
import org.jetbrains.compose.resources.stringResource

@Composable
fun GuildSelectorScreen(
    modifier: Modifier = Modifier,
    guilds: List<Guild>,
    isLoading: Boolean,
    errorMessage: String?,
    searchQuery: String,
    onEvent: (GuildSelectorEvent) -> Unit = {},
) {
    Scaffold(
        modifier = modifier.fillMaxSize(),
        containerColor = MmBackground,
        topBar = {
            MTopBar(
                modifier = Modifier.fillMaxWidth(),
                title = stringResource(Res.string.select_guild),
                subTitle = stringResource(Res.string.available_guilds) + " ${guilds.size}",
                navigationIcon = {
                    //TODO profile adatok kellenek ide
                    Avatar(
                        name = "",
                        imageUrl = "",
                        shape = CircleShape,
                        size = 44.dp,
                        backgroundColor = MmPrimary,
                        contentColor = MmTextPrimary,
                        avatarOnClick = { onEvent(GuildSelectorEvent.AvatarClicked) }
                    )
                },
                actions = {
                    RefreshButton(onClick = {
                        onEvent(GuildSelectorEvent.RefreshClicked)
                    })
                },
            )
        }
    ) { innerPadding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(innerPadding),
            horizontalAlignment = Alignment.Start,
        ) {

            HorizontalDivider()
            SearchBar(
                value = searchQuery,
                onValueChange = { onEvent(GuildSelectorEvent.SearchQueryChanged(it)) },
                placeholder = stringResource(CoreUiRes.string.search_placeholder),
                modifier = Modifier.padding(horizontal = 12.dp, vertical = 16.dp),
            )
            when {
                isLoading -> GuildSelectorPlaceholder(
                    title = stringResource(Res.string.guild_placeholder_title),
                    contentText = stringResource(Res.string.guild_loading_message),
                )
                errorMessage != null -> GuildSelectorPlaceholder(
                    title = stringResource(Res.string.guild_placeholder_title),
                    contentText = errorMessage,
                )
                guilds.isEmpty() -> GuildSelectorPlaceholder(
                    title = stringResource(Res.string.guild_placeholder_title),
                    contentText = stringResource(Res.string.guild_empty_message),
                )
                else -> LazyColumn(
                    modifier = Modifier.fillMaxWidth().weight(1f),
                    contentPadding = PaddingValues(12.dp, 0.dp, 12.dp, 12.dp),
                    verticalArrangement = Arrangement.spacedBy(5.dp),
                ) {
                    items(guilds) { guild ->
                        GuildListItem(
                            guild = guild,
                            onClick = { onEvent(GuildSelectorEvent.GuildClicked(guild.id)) },
                        )
                    }
                }
            }
        }
    }
}


@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildSelectorScreenEmptyListPreview() {
    MelodiasMarioTheme {
        GuildSelectorScreen(
            guilds = emptyList(),
            searchQuery = "",
            isLoading = false,
            errorMessage = null,
        )
    }
}


@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildSelectorScreenIsLoadingPreview() {
    MelodiasMarioTheme {
        GuildSelectorScreen(
            guilds = emptyList(),
            searchQuery = "",
            isLoading = true,
            errorMessage = null,
        )
    }
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildSelectorScreenPreview() {
    MelodiasMarioTheme {
        GuildSelectorScreen(
            guilds = listOf(guild),
            searchQuery = "",
            isLoading = false,
            errorMessage = null,
        )
    }
}

private val guild = Guild(
    id="1309813939563003966",
    name="Business business",
    iconUrl="https://cdn.discordapp.com/icons/1309813939563003966/6efd05e8cd412d4defb7d59941a3a512.webp?size=128",
    accessLevel= GuildAccessLevel.Admin,
    botStatus= BotStatus(
        isOnline = false,
        connectedVoiceChannelName = null,
        connectedVoiceUserCount = 0
    )
)
