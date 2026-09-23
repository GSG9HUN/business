package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.retry_button
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import org.jetbrains.compose.resources.stringResource

@Composable
fun MErrorScreen(
    modifier: Modifier = Modifier,
    errorMessage: String,
    onClick: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current

    Column(
        modifier = modifier
            .fillMaxSize()
            .padding(24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center,
    ) {
        MText(
            text = errorMessage,
            color = colors.errorText,
            fontWeight = FontWeight.Bold,
        )

        Button(
            modifier = Modifier.padding(top = 16.dp),
            onClick = onClick,
            shape = RoundedCornerShape(12.dp),
            border = BorderStroke(1.dp, colors.secondaryButtonOutline),
            colors = ButtonDefaults.buttonColors(
                containerColor = colors.secondaryButtonBackground,
                contentColor = colors.textPrimary,
            ),
        ) {
            MText(
                text = stringResource(Res.string.retry_button),
                color = colors.textPrimary,
                fontWeight = FontWeight.Bold,
            )
        }
    }
}

@Preview
@Composable
private fun MErrorScreenPreview() {
    MErrorScreen(
        errorMessage = "Nem sikerult betolteni az adatokat.",
        onClick = {},
    )
}
