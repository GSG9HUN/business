package com.dc.melodiasmario.core.commonui.designsystem.components.dialog

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.material3.AlertDialog
import androidx.compose.material3.Button
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.OutlinedButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText

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
    onDismiss: () -> Unit,
    dismissText: String? = null,
) {
    val primaryAction = actions.firstOrNull()
    val secondaryActions = actions.drop(1)

    AlertDialog(
        onDismissRequest = onDismiss,
        containerColor = MaterialTheme.colorScheme.surface,
        title = {
            MText(
                modifier = Modifier.fillMaxWidth(),
                text = title,
                color = MaterialTheme.colorScheme.onSurface,
                textAlign = TextAlign.Center,
                fontSize = 20.sp,
                fontWeight = FontWeight.Bold,
            )
        },
        text = {
            MText(
                text = message,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
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
                        color = if (action.isDestructive) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.onSurface,
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
                            color = if (action.isDestructive) MaterialTheme.colorScheme.error else MaterialTheme.colorScheme.onSurfaceVariant,
                        )
                    }
                }
                dismissText?.let { text ->
                    OutlinedButton(onClick = onDismiss) {
                        MText(text = text, color = MaterialTheme.colorScheme.onSurfaceVariant)
                    }
                }
            }
        },
    )
}
