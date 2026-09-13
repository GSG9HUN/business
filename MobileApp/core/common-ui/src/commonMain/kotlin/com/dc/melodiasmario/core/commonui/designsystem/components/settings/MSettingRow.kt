package com.dc.melodiasmario.core.commonui.designsystem.components.settings

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.material3.HorizontalDivider
import androidx.compose.material3.Icon
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_chevron_right
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import org.jetbrains.compose.resources.DrawableResource
import org.jetbrains.compose.resources.painterResource

@Composable
fun MSettingRow(
    icon: DrawableResource,
    title: String,
    subtitle: String,
    modifier: Modifier = Modifier,
    onClick: (() -> Unit)? = null,
    trailingContent: (@Composable () -> Unit)? = null,
) {
    val colors = MelodiasMarioThemeTokens.current
    val rowModifier = if (onClick != null) {
        modifier
            .fillMaxWidth()
            .clickable(onClick = onClick)
            .padding(vertical = 12.dp)
    } else {
        modifier
            .fillMaxWidth()
            .padding(vertical = 12.dp)
    }

    Row(
        modifier = rowModifier,
        verticalAlignment = Alignment.CenterVertically,
        horizontalArrangement = Arrangement.spacedBy(14.dp),
    ) {
        MSettingsIconBox(
            painter = painterResource(icon),
        )

        Column(
            modifier = Modifier.weight(1f),
        ) {
            MText(
                text = title,
                color = colors.textPrimary,
                fontWeight = FontWeight.Bold,
                textAlign = TextAlign.Start,
            )
            MText(
                text = subtitle,
                color = colors.subtitle,
                textAlign = TextAlign.Start,
            )
        }

        if (trailingContent != null) {
            trailingContent()
        } else {
            Icon(
                painter = painterResource(Res.drawable.ic_profile_chevron_right),
                contentDescription = null,
                modifier = Modifier.size(18.dp),
                tint = colors.textPrimary,
            )
        }
    }

    HorizontalDivider(
        thickness = 1.dp,
        color = colors.divider,
    )
}
