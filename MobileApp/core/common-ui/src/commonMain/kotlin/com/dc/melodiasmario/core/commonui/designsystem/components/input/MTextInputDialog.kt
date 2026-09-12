package com.dc.melodiasmario.core.commonui.designsystem.components.input

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.interaction.MutableInteractionSource
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.text.BasicTextField
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.TextStyle
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.commonui.designsystem.theme.MmSurfacePreviewColor

@Composable
fun MTextInputDialog(
    title: String,
    value: String,
    onValueChange: (String) -> Unit,
    onDismiss: () -> Unit,
    onConfirm: () -> Unit,
    modifier: Modifier = Modifier,
    placeholder: String = "",
    confirmText: String,
    dismissText: String,
    enabled: Boolean = true,
    canConfirm: Boolean = true,
) {
    val colors = MelodiasMarioThemeTokens.current

    AlertDialog(
        modifier = modifier,
        onDismissRequest = onDismiss,
        containerColor = colors.surface,
        title = {
            MText(
                text = title,
                color = colors.textPrimary,
                textAlign = TextAlign.Start,
            )
        },
        text = {
            Surface(
                modifier = Modifier.fillMaxWidth(),
                shape = androidx.compose.foundation.shape.RoundedCornerShape(12.dp),
                color = colors.elevated,
                border = BorderStroke(1.dp, colors.outline),
            ) {
                BasicTextField(
                    value = value,
                    onValueChange = onValueChange,
                    enabled = enabled,
                    singleLine = true,
                    textStyle = TextStyle(color = colors.textPrimary),
                    interactionSource = remember { MutableInteractionSource() },
                    modifier = Modifier
                        .fillMaxWidth()
                        .padding(horizontal = 14.dp, vertical = 13.dp),
                    decorationBox = { innerTextField ->
                        Box(
                            modifier = Modifier.fillMaxWidth(),
                            contentAlignment = Alignment.CenterStart,
                        ) {
                            if (value.isBlank()) {
                                MText(
                                    text = placeholder,
                                    color = colors.textMuted,
                                    textAlign = TextAlign.Start,
                                )
                            }
                            innerTextField()
                        }
                    },
                )
            }
        },
        confirmButton = {
            Button(
                onClick = onConfirm,
                enabled = enabled && canConfirm,
            ) {
                MText(text = confirmText, color = colors.textPrimary)
            }
        },
        dismissButton = {
            OutlinedButton(onClick = onDismiss) {
                MText(text = dismissText, color = colors.textSecondary)
            }
        },
    )
}

@Preview(showBackground = true, backgroundColor = MmSurfacePreviewColor)
@Composable
private fun MTextInputDialogPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        var value by remember { mutableStateOf("Morning Mix") }

        MTextInputDialog(
            title = "Rename playlist",
            value = value,
            onValueChange = { value = it },
            onDismiss = {},
            onConfirm = {},
            placeholder = "Playlist name",
            confirmText = "Rename",
            dismissText = "Cancel",
            canConfirm = value.trim().isNotEmpty(),
        )
    }
}
