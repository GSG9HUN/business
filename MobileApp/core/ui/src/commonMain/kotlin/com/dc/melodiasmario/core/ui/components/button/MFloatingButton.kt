package com.dc.melodiasmario.core.ui.components.button

import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.FloatingActionButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens

@Composable
fun MFloatingButton(
    modifier: Modifier = Modifier,
    size: Dp = 72.dp,
    shape: RoundedCornerShape = RoundedCornerShape(22.dp),
    icon: @Composable () -> Unit,
    onClick: () -> Unit = {},
) {
    FloatingActionButton(
        modifier = modifier.size(size),
        onClick = onClick,
        containerColor = MelodiasMarioThemeTokens.current.primary,
        contentColor = MelodiasMarioThemeTokens.current.textPrimary,
        shape = shape,
    ) {
        icon()
    }
}