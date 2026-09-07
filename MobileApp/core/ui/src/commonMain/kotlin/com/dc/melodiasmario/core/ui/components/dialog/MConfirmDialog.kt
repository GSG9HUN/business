package com.dc.melodiasmario.core.ui.components.dialog

import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeMode
import com.dc.melodiasmario.core.ui.theme.MelodiasMarioThemeTokens
import com.dc.melodiasmario.core.ui.theme.MmSurfacePreviewColor

@Composable
fun MConfirmDialog(
    title: String,
    message: String,
    confirmText: String,
    dismissText: String,
    onConfirm: () -> Unit,
    onDismiss: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
    isDestructive: Boolean = false,
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
            MText(
                text = message,
                color = colors.textSecondary,
                textAlign = TextAlign.Start,
            )
        },
        confirmButton = {
            Button(
                enabled = enabled,
                onClick = onConfirm,
            ) {
                MText(
                    text = confirmText,
                    color = if (isDestructive) colors.dangerText else colors.textPrimary,
                )
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
private fun MConfirmDialogPreview() {
    MelodiasMarioTheme(themeMode = MelodiasMarioThemeMode.Dark) {
        MConfirmDialog(
            title = "Delete playlist",
            message = "Delete \"Morning Mix\"?",
            confirmText = "Delete",
            dismissText = "Cancel",
            onConfirm = {},
            onDismiss = {},
            isDestructive = true,
        )
    }
}
