package com.dc.melodiasmario.core.commonui.feedback.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.layout.widthIn
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.LiveRegionMode
import androidx.compose.ui.semantics.SemanticsProperties.LiveRegion
import androidx.compose.ui.semantics.liveRegion
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.feedback.model.MToastData
import com.dc.melodiasmario.core.commonui.feedback.model.MToastType

@Composable
fun MToast(
    modifier: Modifier = Modifier,
    data: MToastData,
) {
    val statusStyle = data.type.statusStyle()

    Surface(
        modifier = modifier
            .semantics {
                liveRegion = when (data.type) {
                    MToastType.Error -> LiveRegionMode.Assertive
                    else -> LiveRegionMode.Polite
                }
            }
            .padding(horizontal = 16.dp)
            .widthIn(max = 560.dp)
            .fillMaxWidth(),
        shape = RoundedCornerShape(16.dp),
        color = MaterialTheme.colorScheme.surfaceContainerHigh,
        border = BorderStroke(1.dp, statusStyle.borderColor),
        shadowElevation = 10.dp,
    ) {
        Row(
            modifier = Modifier.padding(horizontal = 14.dp, vertical = 12.dp),
            horizontalArrangement = Arrangement.spacedBy(12.dp),
            verticalAlignment = Alignment.CenterVertically,
        ) {
            ToastStatusMark(
                label = statusStyle.markLabel,
                backgroundColor = statusStyle.markBackgroundColor,
                contentColor = statusStyle.contentColor,
            )

            MText(
                modifier = Modifier.weight(1f),
                text = data.message,
                color = MaterialTheme.colorScheme.onSurface,
                textAlign = TextAlign.Start,
                fontWeight = FontWeight.SemiBold,
                lineHeight = 18.sp,
            )
        }
    }
}

@Composable
private fun ToastStatusMark(
    label: String,
    backgroundColor: Color,
    contentColor: Color,
) {
    Surface(
        modifier = Modifier.size(28.dp),
        shape = CircleShape,
        color = backgroundColor,
    ) {
        Box(contentAlignment = Alignment.Center) {
            MText(
                text = label,
                color = contentColor,
                fontWeight = FontWeight.Bold,
                lineHeight = 14.sp,
            )
        }
    }
}

private data class ToastColors(
    val markLabel: String,
    val markBackgroundColor: Color,
    val contentColor: Color,
    val borderColor: Color,
)

@Composable
private fun MToastType.statusStyle(): ToastColors {

    return when (this) {
        MToastType.Success -> ToastColors(
            markLabel = "OK",
            markBackgroundColor = MaterialTheme.colorScheme.tertiaryContainer,
            contentColor = MaterialTheme.colorScheme.onTertiaryContainer,
            borderColor = MaterialTheme.colorScheme.onTertiaryContainer.copy(alpha = 0.35f),
        )

        MToastType.Error -> ToastColors(
            markLabel = "!",
            markBackgroundColor = MaterialTheme.colorScheme.errorContainer,
            contentColor = MaterialTheme.colorScheme.onErrorContainer,
            borderColor = MaterialTheme.colorScheme.onErrorContainer.copy(alpha = 0.35f),
        )

        MToastType.Warning -> ToastColors(
            markLabel = "!",
            markBackgroundColor = Color(0x26F6C343),
            contentColor = Color(0xFF996A00),
            borderColor = Color(0x66F6C343),
        )

        MToastType.Info -> ToastColors(
            markLabel = "i",
            markBackgroundColor = MaterialTheme.colorScheme.primary.copy(alpha = 0.18f),
            contentColor = MaterialTheme.colorScheme.secondary,
            borderColor = MaterialTheme.colorScheme.outline,
        )
    }
}
