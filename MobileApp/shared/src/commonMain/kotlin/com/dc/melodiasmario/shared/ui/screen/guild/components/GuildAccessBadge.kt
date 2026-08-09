package com.dc.melodiasmario.shared.ui.screen.guild.components

import androidx.compose.foundation.layout.padding
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.shared.domain.guild.model.GuildAccessLevel
import com.dc.melodiasmario.shared.ui.common.MText
import com.dc.melodiasmario.shared.ui.theme.MmPrimary
import com.dc.melodiasmario.shared.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.shared.ui.theme.MmSurfaceRed
import com.dc.melodiasmario.shared.ui.theme.MmTextPrimary

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

    Surface(
        modifier = modifier,
        shape = RoundedCornerShape(999.dp),
        color = backgroundColor,
    ) {
        MText(
            modifier = Modifier.padding(horizontal = 10.dp, vertical = 6.dp),
            text = text,
            color = MmTextPrimary,
        )
    }
}
