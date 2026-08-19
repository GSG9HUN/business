package com.dc.melodiasmario.feature.guild.ui

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.feature.guild.domain.model.Guild
import com.dc.melodiasmario.feature.guild.presentation.GuildSelectorEvent
import com.dc.melodiasmario.core.ui.components.Header
import com.dc.melodiasmario.core.ui.components.SearchBar
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MmBackground
import com.dc.melodiasmario.core.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.feature.guild.ui.components.GuildListItem
import com.dc.melodiasmario.core.ui.generated.resources.Res
import com.dc.melodiasmario.core.ui.generated.resources.available_guilds
import com.dc.melodiasmario.core.ui.generated.resources.search_placeholder
import com.dc.melodiasmario.core.ui.generated.resources.select_guild
import org.jetbrains.compose.resources.stringResource

@Composable
fun GuildSelectorScreen(
    modifier: Modifier = Modifier,
    guilds: List<Guild>,
    searchQuery: String,
    onEvent: (GuildSelectorEvent) -> Unit = {},
) {
    Surface(
        modifier = modifier.fillMaxSize(),
        color = MmBackground,
    ) {
        Column(
            modifier = Modifier.fillMaxSize(),
            horizontalAlignment = Alignment.Start,
        ) {
            Header(
                title = stringResource(Res.string.select_guild),
                subTitle = stringResource(Res.string.available_guilds) + " ${guilds.size}",
                refreshButtonOnClick = { onEvent(GuildSelectorEvent.RefreshClicked) },
                avatarOnClick = { onEvent(GuildSelectorEvent.AvatarClicked) },
            )
            HorizontalDivider()

            SearchBar(
                value = searchQuery,
                onValueChange = { onEvent(GuildSelectorEvent.SearchQueryChanged(it)) },
                placeholder = stringResource(Res.string.search_placeholder),
                modifier = Modifier.padding(horizontal = 12.dp, vertical = 16.dp),
            )
            LazyColumn(
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


@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildSelectorScreenPreview() {
    MelodiasMarioTheme {
        GuildSelectorScreen(
            guilds = emptyList(),
            searchQuery = "",
        )
    }
}
