package com.dc.melodiasmario.feature.playlisttrack.ui.components

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.ModalBottomSheet
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.TextButton
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.dialog.MMoreAction
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText

/**
 * Renders playlist track actions sheet from the supplied parameters and forwards interaction callbacks.
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
internal fun PlaylistTrackActionsSheet(
    title: String,
    message: String,
    actions: List<MMoreAction>,
    onDismiss: () -> Unit,
    dismissText: String
): Unit {
    ModalBottomSheet(onDismissRequest = onDismiss) {
        Column(
            modifier = Modifier.fillMaxWidth()
                .padding(horizontal = 20.dp)
                .padding(bottom = 24.dp),
            verticalArrangement = Arrangement.spacedBy(space = 8.dp)
        ) {
            MText(
                text = title,
                color = MaterialTheme.colorScheme.onSurface,
                fontWeight = FontWeight.Bold,
                textAlign = TextAlign.Start
            )
            MText(text = message, textAlign = TextAlign.Start)
            actions.forEach { action ->
                TextButton(onClick = action.onClick, enabled = action.enabled, modifier = Modifier.fillMaxWidth()) {
                    MText(
                        text = action.text,
                        modifier = Modifier.fillMaxWidth(),
                        textAlign = TextAlign.Start,
                        color = when {
                            !action.enabled -> MaterialTheme.colorScheme.onSurfaceVariant
                            action.isDestructive -> MaterialTheme.colorScheme.error
                            else -> MaterialTheme.colorScheme.onSurface
                        }
                    )
                }
            }
            OutlinedButton(onClick = onDismiss, modifier = Modifier.fillMaxWidth()) {
                MText(text = dismissText)
            }
        }
    }
}
