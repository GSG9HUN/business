package com.dc.melodiasmario.core.commonui.components

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.retry_button
import org.jetbrains.compose.resources.stringResource

/**
 * Renders error screen from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun MErrorScreen(
    modifier: Modifier = Modifier,
    errorMessage: String,
    onClick: () -> Unit
): Unit {
    Column(
        modifier = modifier
            .fillMaxSize()
            .padding(all = 24.dp),
        horizontalAlignment = Alignment.CenterHorizontally,
        verticalArrangement = Arrangement.Center
    ) {
        MText(
            text = errorMessage,
            color = MaterialTheme.colorScheme.onErrorContainer,
            fontWeight = FontWeight.Bold
        )

        Button(
            modifier = Modifier.padding(top = 16.dp),
            onClick = onClick,
            shape = RoundedCornerShape(size = 12.dp),
            border = BorderStroke(width = 1.dp, color = MaterialTheme.colorScheme.outline),
            colors = ButtonDefaults.buttonColors(
                containerColor = MaterialTheme.colorScheme.surfaceContainerHigh,
                contentColor = MaterialTheme.colorScheme.onSurface
            )
        ) {
            MText(
                text = stringResource(resource = Res.string.retry_button),
                color = MaterialTheme.colorScheme.onSurface,
                fontWeight = FontWeight.Bold
            )
        }
    }
}

@Preview
@Composable
private fun MErrorScreenPreview() {
    MErrorScreen(
        errorMessage = "Nem sikerult betolteni az adatokat.",
        onClick = {}
    )
}
