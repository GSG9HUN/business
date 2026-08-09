package com.dc.melodiasmario.shared.ui.screen.auth.components

import androidx.compose.foundation.Image
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import mobileapp.shared.generated.resources.Res
import com.dc.melodiasmario.shared.ui.theme.MmCyan
import com.dc.melodiasmario.shared.ui.theme.MmPrimaryAlt
import mobileapp.shared.generated.resources.app_name
import mobileapp.shared.generated.resources.melodias_mario_note
import org.jetbrains.compose.resources.painterResource
import org.jetbrains.compose.resources.stringResource

@Composable
fun MelodiasLogo(modifier: Modifier = Modifier) {
    Box(
        modifier = modifier
            .size(86.dp)
            .clip(RoundedCornerShape(24.dp))
            .background(
                Brush.linearGradient(
                    colors = listOf(MmPrimaryAlt, MmCyan),
                ),
            ),
        contentAlignment = Alignment.Center,
    ) {
        Image(
            painter = painterResource(Res.drawable.melodias_mario_note),
            contentDescription = stringResource(Res.string.app_name),
            modifier = Modifier.size(200.dp),
        )
    }
}

@Preview
@Composable
private fun MelodiasLogoPreview() {
    MelodiasLogo()
}