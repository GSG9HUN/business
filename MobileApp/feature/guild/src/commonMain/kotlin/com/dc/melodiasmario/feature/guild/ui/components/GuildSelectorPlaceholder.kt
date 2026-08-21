package com.dc.melodiasmario.feature.guild.ui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.MText
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmSurface
import com.dc.melodiasmario.core.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.core.ui.theme.MmTextMuted
import com.dc.melodiasmario.core.ui.theme.MmTextPrimary

@Composable
fun GuildSelectorPlaceholder(
    title: String,
    contentText: String,
) {
    Surface(
        modifier = Modifier.fillMaxWidth().padding(PaddingValues(12.dp, 0.dp, 12.dp, 12.dp)),
        shape = RoundedCornerShape(12.dp),
        color = MmSurface,
        border = BorderStroke(1.dp, MmSurfaceOutline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp),
        ) {
            Surface(
                shape = RoundedCornerShape(10.dp),
                color = MmPrimary,
            ) {
                MText(
                    modifier = Modifier.padding(horizontal = 12.dp, vertical = 9.dp),
                    text = "DC",
                    color = MmTextPrimary,
                    fontWeight = FontWeight.Bold,
                )
            }

            Column(
                modifier = Modifier.weight(1f),
                verticalArrangement = Arrangement.spacedBy(4.dp),
            ) {
                MText(
                    text = title,
                    color = MmTextPrimary,
                    fontWeight = FontWeight.Bold,
                    textAlign = TextAlign.Start,
                )
                MText(
                    text = contentText,
                    color = MmTextMuted,
                    textAlign = TextAlign.Start,
                )
            }
        }
    }
}
