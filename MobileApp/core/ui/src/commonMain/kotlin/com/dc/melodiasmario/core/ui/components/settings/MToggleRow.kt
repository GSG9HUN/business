package com.dc.melodiasmario.core.ui.components.settings

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.HorizontalDivider
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens
import org.jetbrains.compose.resources.DrawableResource
import org.jetbrains.compose.resources.painterResource

@Composable
fun MToggleRow(
    icon: DrawableResource,
    title: String,
    subtitle: String,
    checked: Boolean,
    onCheckedChange: (Boolean) -> Unit,
    modifier: Modifier = Modifier,
) {
    val colors = MelodiasMarioThemeTokens.current

    Row(
        modifier = modifier
            .fillMaxWidth()
            .padding(vertical = 12.dp),
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

        MSwitchButton(
            checked = checked,
            onCheckedChange = onCheckedChange,
        )
    }

    HorizontalDivider(
        thickness = 1.dp,
        color = colors.divider,
    )
}
