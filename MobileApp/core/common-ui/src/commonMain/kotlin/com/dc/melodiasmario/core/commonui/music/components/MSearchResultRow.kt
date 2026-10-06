package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.runtime.Composable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.clickable
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.sp
import androidx.compose.ui.Alignment
import androidx.compose.foundation.BorderStroke
import com.dc.melodiasmario.core.commonui.components.MArtwork
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.music.model.MAddMusicLabels
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchResultUi

@Composable
fun MSearchResultRow(
    result: MMusicSearchResultUi,
    labels: MAddMusicLabels,
    selected: Boolean,
    onClick: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current
    Surface(
        modifier = Modifier.fillMaxWidth().clickable(onClick = onClick),
        shape = RoundedCornerShape(8.dp),
        color = if (selected) colors.elevated else colors.surface,
        border = BorderStroke(1.dp, if (selected) colors.primary else colors.outline),
    ) {
        Row(
            modifier = Modifier.fillMaxWidth().padding(10.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(10.dp),
        ) {
            MArtwork(
                modifier = Modifier.size(44.dp),
                iconSize = 22.dp,
                imageUrl = result.thumbnailUrl,
            )
            Column(modifier = Modifier.weight(1f)) {
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.title,
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    fontWeight = FontWeight.Bold,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(
                    modifier = Modifier.fillMaxWidth(),
                    text = result.subtitle(labels),
                    color = colors.textMuted,
                    textAlign = TextAlign.Start,
                    fontSize = 12.sp,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
            }
        }
    }
}

