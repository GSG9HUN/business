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
import com.dc.melodiasmario.feature.guild.domain.model.guild.Guild
import com.dc.melodiasmario.feature.guild.presentation.GuildSelectorEvent
import com.dc.melodiasmario.core.ui.components.MTopBar
import com.dc.melodiasmario.core.ui.components.button.MRefreshButton
import com.dc.melodiasmario.core.ui.components.display.MAvatar
import com.dc.melodiasmario.core.ui.components.input.MSearchBar
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.feature.guild.domain.model.currentuser.CurrentUser
import com.dc.melodiasmario.feature.guild.ui.components.GuildListItem
import com.dc.melodiasmario.feature.guild.domain.model.guild.BotStatus
import com.dc.melodiasmario.feature.guild.domain.model.guild.GuildAccessLevel
import com.dc.melodiasmario.feature.guild.generated.resources.Res
import com.dc.melodiasmario.feature.guild.generated.resources.guild_available_count
import com.dc.melodiasmario.feature.guild.generated.resources.guild_empty_message
import com.dc.melodiasmario.feature.guild.generated.resources.guild_loading_message
import com.dc.melodiasmario.feature.guild.generated.resources.guild_placeholder_title
import com.dc.melodiasmario.feature.guild.generated.resources.guild_search_placeholder
import com.dc.melodiasmario.feature.guild.generated.resources.guild_select_title
import com.dc.melodiasmario.feature.guild.ui.components.GuildSelectorPlaceholder
import org.jetbrains.compose.resources.stringResource

@Composable
fun GuildSelectorScreen(
    modifier: Modifier = Modifier,
    currentUser: CurrentUser,
    guilds: List<Guild>,
    isLoading: Boolean,
    errorMessage: String?,
    searchQuery: String,
    onEvent: (GuildSelectorEvent) -> Unit = {},
) {
    val colors = MelodiasMarioThemeTokens.current

    Scaffold(
        modifier = modifier.fillMaxSize(),
        containerColor = colors.background,
        topBar = {
            MTopBar(
                modifier = Modifier.fillMaxWidth(),
                title = stringResource(Res.string.guild_select_title),
                subTitle = stringResource(Res.string.guild_available_count, guilds.size),
                navigationIcon = {
                    MAvatar(
                        name = currentUser.displayName,
                        imageUrl = currentUser.avatarUrl,
                        shape = CircleShape,
                        size = 44.dp,
                        backgroundColor = colors.primary,
                        contentColor = colors.textPrimary,
                        avatarOnClick = { onEvent(GuildSelectorEvent.AvatarClicked) },
                    )
                },
                actions = {
                    MRefreshButton(onClick = {
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
            MSearchBar(
                query = searchQuery,
                onQueryChange = { onEvent(GuildSelectorEvent.SearchQueryChanged(it)) },
                placeholder = stringResource(Res.string.guild_search_placeholder),
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
            currentUser = CurrentUser(
                id = "1234567890",
                displayName = "John Doe",
                avatarUrl = "https://example.com/avatar.jpg",
                username = "johndoe",
            )
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
            currentUser = CurrentUser(
                id = "1234567890",
                displayName = "John Doe",
                avatarUrl = "https://example.com/avatar.jpg",
                username = "johndoe",
            )
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
            currentUser = CurrentUser(
                id = "1234567890",
                displayName = "John Doe",
                avatarUrl = "https://example.com/avatar.jpg",
                username = "johndoe",
            )
        )
    }
}

private val guild = Guild(
    id = "1309813939563003966",
    name = "Business business",
    iconUrl = "https://cdn.discordapp.com/icons/1309813939563003966/6efd05e8cd412d4defb7d59941a3a512.webp?size=128",
    accessLevel = GuildAccessLevel.Admin,
    botStatus = BotStatus(
        isOnline = false,
        connectedVoiceChannelName = null,
        connectedVoiceUserCount = 0,
    ),
)
