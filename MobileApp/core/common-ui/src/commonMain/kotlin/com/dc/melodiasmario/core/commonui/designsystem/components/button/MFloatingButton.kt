package com.dc.melodiasmario.core.commonui.designsystem.components.button

import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.Dp
import androidx.compose.ui.unit.dp

/**
 * Renders floating button from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MFloatingButton(
    modifier: Modifier = Modifier,
    size: Dp = 72.dp,
    shape: RoundedCornerShape = RoundedCornerShape(size = 22.dp),
    icon: @Composable () -> Unit,
    onClick: () -> Unit = {}
): Unit {
    FloatingActionButton(
        modifier = modifier.size(size = size),
        onClick = onClick,
        containerColor = MaterialTheme.colorScheme.primary,
        contentColor = MaterialTheme.colorScheme.onPrimary,
        shape = shape
    ) {
        icon()
    }
}
