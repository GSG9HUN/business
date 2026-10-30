package com.dc.melodiasmario.feature.login.ui.components

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.app_name
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.melodias_mario_note
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

/**
 * Renders melodias logo from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MelodiasLogo(modifier: Modifier = Modifier): Unit {
    Box(
        modifier = modifier
            .size(size = 86.dp)
            .clip(shape = RoundedCornerShape(size = 24.dp))
            .background(
                Brush.linearGradient(
                    colors = listOf(MaterialTheme.colorScheme.primary, MaterialTheme.colorScheme.secondary)
                )
            ),
        contentAlignment = Alignment.Center
    ) {
        Image(
            painter = painterResource(resource = Res.drawable.melodias_mario_note),
            contentDescription = stringResource(resource = Res.string.app_name),
            modifier = Modifier.size(size = 200.dp)
        )
    }
}

@Preview
@Composable
private fun MelodiasLogoPreview() {
    MelodiasLogo()
}
