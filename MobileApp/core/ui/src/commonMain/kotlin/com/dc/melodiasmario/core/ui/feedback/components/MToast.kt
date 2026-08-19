package com.dc.melodiasmario.core.ui.feedback.components

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
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.feedback.model.MToastData
import com.dc.melodiasmario.core.ui.feedback.model.MToastType
import com.dc.melodiasmario.core.ui.theme.MmCyan
import com.dc.melodiasmario.core.ui.theme.MmElevated
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmProfileErrorBackground
import com.dc.melodiasmario.core.ui.theme.MmProfileErrorText
import com.dc.melodiasmario.core.ui.theme.MmProfileSuccessBackground
import com.dc.melodiasmario.core.ui.theme.MmProfileSuccessText
import com.dc.melodiasmario.core.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.core.ui.theme.MmTextPrimary

@Composable
fun MToast(
    modifier: Modifier = Modifier,
    data: MToastData,
) {
    val colors = data.type.colors()

    Surface(
        modifier = modifier
            .padding(horizontal = 16.dp)
            .widthIn(max = 560.dp)
            .fillMaxWidth(),
        shape = RoundedCornerShape(16.dp),
        color = MmElevated,
        border = BorderStroke(1.dp, colors.borderColor),
        shadowElevation = 10.dp,
    ) {
        Row(
            modifier = Modifier.padding(horizontal = 14.dp, vertical = 12.dp),
            horizontalArrangement = Arrangement.spacedBy(12.dp),
            verticalAlignment = Alignment.CenterVertically,
        ) {
            ToastStatusMark(
                label = colors.markLabel,
                backgroundColor = colors.markBackgroundColor,
                contentColor = colors.contentColor,
            )

            MText(
                modifier = Modifier.weight(1f),
                text = data.message,
                color = MmTextPrimary,
                textAlign = TextAlign.Start,
                fontWeight = FontWeight.SemiBold,
                lineHeight = 18,
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
                lineHeight = 14,
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

private fun MToastType.colors(): ToastColors = when (this) {
    MToastType.Success -> ToastColors(
        markLabel = "OK",
        markBackgroundColor = MmProfileSuccessBackground,
        contentColor = MmProfileSuccessText,
        borderColor = MmProfileSuccessText.copy(alpha = 0.35f),
    )

    MToastType.Error -> ToastColors(
        markLabel = "!",
        markBackgroundColor = MmProfileErrorBackground,
        contentColor = MmProfileErrorText,
        borderColor = MmProfileErrorText.copy(alpha = 0.35f),
    )

    MToastType.Warning -> ToastColors(
        markLabel = "!",
        markBackgroundColor = Color(0x26F6C343),
        contentColor = Color(0xFFFFD36A),
        borderColor = Color(0x66F6C343),
    )

    MToastType.Info -> ToastColors(
        markLabel = "i",
        markBackgroundColor = MmPrimary.copy(alpha = 0.18f),
        contentColor = MmCyan,
        borderColor = MmSurfaceOutline,
    )
}
