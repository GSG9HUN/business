package com.dc.melodiasmario.feature.guild.ui.components

import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.ui.components.display.MBadge
import com.dc.melodiasmario.feature.guild.domain.model.GuildAccessLevel
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.core.ui.theme.MmSurfaceRed

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
        GuildAccessLevel.Admin -> MmPrimary.copy(alpha = 0.18f)
        GuildAccessLevel.DJ -> MmSurfaceOutline.copy(alpha = 0.35f)
        GuildAccessLevel.ReadOnly -> MmSurfaceRed.copy(alpha = 0.35f)
    }

    MBadge(
        modifier = modifier,
        text = text,
        backgroundColor = backgroundColor,
    )
}
