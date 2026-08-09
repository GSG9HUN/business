package com.dc.melodiasmario.shared.ui.screen.guild

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
import com.dc.melodiasmario.shared.domain.guild.model.Guild
import com.dc.melodiasmario.shared.presentation.guild.GuildSelectorEvent
import com.dc.melodiasmario.shared.ui.common.Header
import com.dc.melodiasmario.shared.ui.common.SearchBar
import com.dc.melodiasmario.shared.ui.screen.guild.components.GuildListItem
import com.dc.melodiasmario.shared.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.shared.ui.theme.MmBackground
import com.dc.melodiasmario.shared.ui.theme.MmBackgroundPreviewColor
import mobileapp.shared.generated.resources.Res
import mobileapp.shared.generated.resources.available_guilds
import mobileapp.shared.generated.resources.search_placeholder
import mobileapp.shared.generated.resources.select_guild
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
