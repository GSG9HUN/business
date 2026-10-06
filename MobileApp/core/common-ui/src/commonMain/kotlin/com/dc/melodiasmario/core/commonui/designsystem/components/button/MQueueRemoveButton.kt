package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.semantics.contentDescription
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

@Composable
fun MQueueRemoveButton(
    contentDescription: String,
    onClick: () -> Unit,
    modifier: Modifier = Modifier,
) {
    val colors = MelodiasMarioThemeTokens.current

    Surface(
        modifier = modifier
            .size(36.dp)
            .semantics { this.contentDescription = contentDescription },
        shape = RoundedCornerShape(9.dp),
        color = colors.dangerBackground,
        border = BorderStroke(1.dp, colors.dangerText.copy(alpha = 0.28f)),
        onClick = onClick,
    ) {
        Box(contentAlignment = Alignment.Center) {
            MText(
                text = "X",
                color = colors.dangerText,
                fontSize = 15.sp,
                fontWeight = FontWeight.Bold,
            )
        }
    }
}
