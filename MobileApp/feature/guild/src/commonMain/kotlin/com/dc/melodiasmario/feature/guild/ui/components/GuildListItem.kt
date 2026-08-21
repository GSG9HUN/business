package com.dc.melodiasmario.feature.guild.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.feature.guild.domain.model.Guild
import com.dc.melodiasmario.feature.guild.domain.model.GuildAccessLevel
import com.dc.melodiasmario.feature.guild.domain.model.BotStatus
import com.dc.melodiasmario.core.ui.components.display.MAvatar
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.theme.MmBackgroundPreviewColor
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmSurface
import com.dc.melodiasmario.core.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.core.ui.theme.MmTextPrimary
import com.dc.melodiasmario.core.ui.theme.MmTextSecondary
import com.dc.melodiasmario.feature.guild.generated.resources.Res
import com.dc.melodiasmario.feature.guild.generated.resources.guild_bot_status_unknown
import com.dc.melodiasmario.feature.guild.generated.resources.guild_in_voice_channel
import com.dc.melodiasmario.feature.guild.generated.resources.guild_is_offline
import com.dc.melodiasmario.feature.guild.generated.resources.guild_is_online
import org.jetbrains.compose.resources.stringResource

@Composable
fun GuildListItem(
    guild: Guild,
    modifier: Modifier = Modifier,
    onClick: () -> Unit = {},
) {
    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(12.dp),
        color = MmSurface,
        border = BorderStroke(1.dp, MmSurfaceOutline),
        onClick = onClick,
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            MAvatar(
                name = guild.name,
                imageUrl = guild.iconUrl,
                shape = RoundedCornerShape(10.dp),
                size = 44.dp,
                backgroundColor = MmPrimary,
                contentColor = MmTextPrimary,
            )

            Column(
                modifier = Modifier.weight(1f),
                verticalArrangement = Arrangement.Center,
            ) {
                MText(
                    modifier = Modifier.padding(start = 5.dp),
                    text = guild.name,
                    color = MmTextPrimary,
                    fontWeight = FontWeight.Bold,
                )
                MText(
                    modifier = Modifier.padding(start = 5.dp),
                    text = guild.statusText(
                        isOnlineText = stringResource(Res.string.guild_is_online),
                        isOfflineText = stringResource(Res.string.guild_is_offline),
                        unknownText = stringResource(Res.string.guild_bot_status_unknown),
                        connectedInVoiceSuffix = stringResource(Res.string.guild_in_voice_channel),
                    ),
                    color = MmTextSecondary,
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
        modifier = Modifier.padding(10.dp),
        guild = previewGuild(),
    )
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildListItemPreviewWithNoConnectedUserCount() {
    GuildListItem(
        modifier = Modifier.padding(10.dp),
        guild = previewGuild(
            botStatus = BotStatus(
                isOnline = true,
                connectedVoiceChannelName = "Random VC name",
                connectedVoiceUserCount = 0,
            ),
            accessLevel = GuildAccessLevel.DJ,
        ),
    )
}

@Preview(showBackground = true, backgroundColor = MmBackgroundPreviewColor)
@Composable
private fun GuildListItemPreviewBotIsOffline() {
    GuildListItem(
        modifier = Modifier.padding(10.dp),
        guild = previewGuild(
            botStatus = BotStatus(isOnline = false, connectedVoiceChannelName = null, connectedVoiceUserCount = 0),
            accessLevel = GuildAccessLevel.Admin,
        ),
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
        connectedVoiceUserCount = 3,
    ),
): Guild = Guild(
    id = id,
    name = name,
    iconUrl = iconUrl,
    accessLevel = accessLevel,
    botStatus = botStatus,
)

private fun Guild.statusText(
    isOnlineText: String,
    isOfflineText: String,
    unknownText: String,
    connectedInVoiceSuffix: String,
): String {
    val status = botStatus ?: return unknownText

    if (!status.isOnline) return isOfflineText

    val parts = mutableListOf(isOnlineText)

    if (status.connectedVoiceUserCount > 0) {
        parts += "${status.connectedVoiceUserCount} $connectedInVoiceSuffix"
        return parts.joinToString(" - ")
    }

    status.connectedVoiceChannelName?.takeIf { it.isNotBlank() }?.let(parts::add)

    return parts.joinToString(" - ")
}
