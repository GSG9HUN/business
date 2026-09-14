package com.dc.melodiasmario.feature.profile.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Button
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.retry_button
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import org.jetbrains.compose.resources.stringResource

@Composable
fun ProfileErrorContent(
    modifier: Modifier = Modifier,
    message: String,
    onRetryClick: () -> Unit,
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
            text = message,
            color = colors.errorText,
            fontWeight = FontWeight.Bold,
        )

        Button(
            onClick = onRetryClick,
            modifier = Modifier.padding(top = 16.dp),
        ) {
            MText(text = stringResource(Res.string.retry_button))
        }
    }
}
