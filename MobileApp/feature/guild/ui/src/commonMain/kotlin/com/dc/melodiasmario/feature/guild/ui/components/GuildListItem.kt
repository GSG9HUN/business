package com.dc.melodiasmario.feature.guild.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MAvatar
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res as CommonUiRes
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_in_voice_channel
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_offline
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_online
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.guild_status_unknown
import com.dc.melodiasmario.core.commonui.designsystem.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.core.commonui.guild.botStatusText
import com.dc.melodiasmario.core.model.guild.BotStatus
import com.dc.melodiasmario.core.model.guild.Guild
import com.dc.melodiasmario.core.model.guild.GuildAccessLevel
import org.jetbrains.compose.resources.stringResource

/**
 * Renders guild list item from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun GuildListItem(
    guild: Guild,
    modifier: Modifier = Modifier,
    onClick: () -> Unit = {}
): Unit {
    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(size = 12.dp),
        color = MaterialTheme.colorScheme.surface,
        border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline),
        onClick = onClick
    ) {
        Row(
            modifier = Modifier.fillMaxWidth()
                .padding(all = 10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(space = 12.dp)
        ) {
            MAvatar(
                name = guild.name,
                imageUrl = guild.iconUrl,
                shape = RoundedCornerShape(size = 10.dp),
                size = 44.dp,
                backgroundColor = MaterialTheme.colorScheme.primary,
                contentColor = MaterialTheme.colorScheme.onPrimary
            )

            Column(
                modifier = Modifier.weight(weight = 1f),
                verticalArrangement = Arrangement.Center
            ) {
                MText(
                    modifier = Modifier.padding(start = 5.dp),
                    text = guild.name,
                    color = MaterialTheme.colorScheme.onSurface,
                    fontWeight = FontWeight.Bold
                )
                MText(
                    modifier = Modifier.padding(start = 5.dp),
                    text = botStatusText(
                        isOnline = guild.botStatus?.isOnline,
                        connectedVoiceChannelName = guild.botStatus?.connectedVoiceChannelName,
                        connectedVoiceUserCount = guild.botStatus?.connectedVoiceUserCount ?: 0,
                        isOnlineText = stringResource(resource = CommonUiRes.string.guild_status_online),
                        isOfflineText = stringResource(resource = CommonUiRes.string.guild_status_offline),
                        unknownText = stringResource(resource = CommonUiRes.string.guild_status_unknown),
                        connectedInVoiceSuffix = stringResource(resource = CommonUiRes.string.guild_status_in_voice_channel)
                    ),
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            GuildAccessBadge(accessLevel = guild.accessLevel)
        }
    }
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildListItemPreview() {
    GuildListItem(
        modifier = Modifier.padding(all = 10.dp),
        guild = previewGuild()
    )
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildListItemPreviewWithNoConnectedUserCount() {
    GuildListItem(
        modifier = Modifier.padding(all = 10.dp),
        guild = previewGuild(
            botStatus = BotStatus(
                isOnline = true,
                connectedVoiceChannelName = "Random VC name",
                connectedVoiceUserCount = 0
            ),
            accessLevel = GuildAccessLevel.DJ
        )
    )
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildListItemPreviewBotIsOffline() {
    GuildListItem(
        modifier = Modifier.padding(all = 10.dp),
        guild = previewGuild(
            botStatus = BotStatus(isOnline = false, connectedVoiceChannelName = null, connectedVoiceUserCount = 0),
            accessLevel = GuildAccessLevel.Admin
        )
    )
}

private fun previewGuild(
    id: String = "1",
    name: String = "DC",
    iconUrl: String? = null,
    accessLevel: GuildAccessLevel = GuildAccessLevel.ReadOnly,
    botStatus: BotStatus? = BotStatus(
        isOnline = true,
        connectedVoiceChannelName = "General",
        connectedVoiceUserCount = 3
    )
): Guild = Guild(
    id = id,
    name = name,
    iconUrl = iconUrl,
    accessLevel = accessLevel,
    botStatus = botStatus
)
