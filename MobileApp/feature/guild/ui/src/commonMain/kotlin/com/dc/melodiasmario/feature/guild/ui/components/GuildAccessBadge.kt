package com.dc.melodiasmario.feature.guild.ui.components

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MBadge
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.model.guild.GuildAccessLevel

@Composable
fun GuildAccessBadge(
    accessLevel: GuildAccessLevel,
    modifier: Modifier = Modifier,
) {
    val colors = MelodiasMarioThemeTokens.current

    val text = when (accessLevel) {
        GuildAccessLevel.Admin -> "Admin"
        GuildAccessLevel.DJ -> "DJ"
        GuildAccessLevel.ReadOnly -> "Read"
    }

    val backgroundColor = when (accessLevel) {
        GuildAccessLevel.Admin -> colors.primary.copy(alpha = 0.18f)
        GuildAccessLevel.DJ -> colors.outline.copy(alpha = 0.35f)
        GuildAccessLevel.ReadOnly -> colors.dangerBackground.copy(alpha = 0.35f)
    }

    MBadge(
        modifier = modifier,
        text = text,
        backgroundColor = backgroundColor,
    )
}
