package com.dc.melodiasmario.feature.guild.ui.components

import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MBadge
import com.dc.melodiasmario.core.model.guild.GuildAccessLevel

@Composable
fun GuildAccessBadge(
    accessLevel: GuildAccessLevel,
    modifier: Modifier = Modifier,
) {
    val text = when (accessLevel) {
        GuildAccessLevel.Admin -> "Admin"
        GuildAccessLevel.DJ -> "DJ"
        GuildAccessLevel.ReadOnly -> "Read"
    }

    val backgroundColor = when (accessLevel) {
        GuildAccessLevel.Admin -> MaterialTheme.colorScheme.primary.copy(alpha = 0.18f)
        GuildAccessLevel.DJ -> MaterialTheme.colorScheme.outline.copy(alpha = 0.35f)
        GuildAccessLevel.ReadOnly -> MaterialTheme.colorScheme.errorContainer.copy(alpha = 0.35f)
    }

    MBadge(
        modifier = modifier,
        text = text,
        backgroundColor = backgroundColor,
    )
}
