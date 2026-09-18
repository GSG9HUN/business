package com.dc.melodiasmario.core.commonui.designsystem.components.dialog

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioThemeTokens

data class MMoreAction(
    val text: String,
    val onClick: () -> Unit,
    val isDestructive: Boolean = false,
    val enabled: Boolean = true,
)

@Composable
fun MMoreActionsDialog(
    title: String,
    message: String,
    actions: List<MMoreAction>,
    dismissText: String,
    onDismiss: () -> Unit,
) {
    val colors = MelodiasMarioThemeTokens.current
    val primaryAction = actions.firstOrNull()
    val secondaryActions = actions.drop(1)

    AlertDialog(
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
            primaryAction?.let { action ->
                Button(
                    onClick = action.onClick,
                    enabled = action.enabled,
                ) {
                    MText(
                        text = action.text,
                        color = if (action.isDestructive) colors.dangerText else colors.textPrimary,
                    )
                }
            }
        },
        dismissButton = {
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                secondaryActions.forEach { action ->
                    OutlinedButton(
                        onClick = action.onClick,
                        enabled = action.enabled,
                    ) {
                        MText(
                            text = action.text,
                            color = if (action.isDestructive) colors.dangerText else colors.textSecondary,
                        )
                    }
                }
                OutlinedButton(onClick = onDismiss) {
                    MText(text = dismissText, color = colors.textSecondary)
                }
            }
        },
    )
}
