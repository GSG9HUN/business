package com.dc.melodiasmario.core.commonui.music.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.DropdownMenu
import androidx.compose.material3.DropdownMenuItem
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.music.model.MMusicSearchProviderUi

@Composable
fun MProviderSelector(
    label: String,
    selectedProvider: MMusicSearchProviderUi?,
    providers: List<MMusicSearchProviderUi>,
    onProviderChanged: (String) -> Unit,
) {
    var expanded by remember { mutableStateOf(false) }
    val colors = MelodiasMarioThemeTokens.current

    Box(modifier = Modifier.fillMaxWidth()) {
        OutlinedButton(
            modifier = Modifier.fillMaxWidth(),
            onClick = { expanded = true },
        ) {
            Row(
                modifier = Modifier.fillMaxWidth(),
                verticalAlignment = Alignment.CenterVertically,
                horizontalArrangement = Arrangement.spacedBy(8.dp),
            ) {
                MText(
                    modifier = Modifier.weight(1f),
                    text = "$label: ${selectedProvider?.name.orEmpty()}",
                    color = colors.textPrimary,
                    textAlign = TextAlign.Start,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                )
                MText(text = "v", color = colors.textSecondary)
            }
        }

        DropdownMenu(
            expanded = expanded,
            onDismissRequest = { expanded = false },
        ) {
            providers.forEach { provider ->
                DropdownMenuItem(
                    text = {
                        MText(
                            text = provider.name,
                            color = if (!provider.canEnqueue) colors.textMuted else colors.textPrimary,
                            textAlign = TextAlign.Start,
                        )
                    },
                    onClick = {
                        expanded = false
                        onProviderChanged(provider.id)
                    },
                )
            }
        }
    }
}