package com.dc.melodiasmario.core.ui.components.button

import androidx.compose.foundation.BorderStroke
import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.Button
import androidx.compose.material3.ButtonDefaults
import androidx.compose.material3.OutlinedButton
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.ui.components.display.MText
import com.dc.melodiasmario.core.ui.theme.MmElevated
import com.dc.melodiasmario.core.ui.theme.MmPrimary
import com.dc.melodiasmario.core.ui.theme.MmSecondaryButtonBackground
import com.dc.melodiasmario.core.ui.theme.MmSecondaryButtonOutline
import com.dc.melodiasmario.core.ui.theme.MmSurfaceOutline
import com.dc.melodiasmario.core.ui.theme.MmTextPrimary

@Composable
fun MFloatingSaveCancelBar(
    saveText: String,
    cancelText: String,
    onSaveClick: () -> Unit,
    onCancelClick: () -> Unit,
    modifier: Modifier = Modifier,
    enabled: Boolean = true,
) {
    Surface(
        modifier = modifier
            .fillMaxWidth()
            .padding(horizontal = 16.dp, vertical = 12.dp),
        shape = RoundedCornerShape(18.dp),
        color = MmElevated,
        border = BorderStroke(1.dp, MmSurfaceOutline),
        shadowElevation = 8.dp,
    ) {
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(10.dp),
            horizontalArrangement = Arrangement.spacedBy(10.dp),
            verticalAlignment = Alignment.CenterVertically,
        ) {
            OutlinedButton(
                modifier = Modifier.weight(1f),
                onClick = onCancelClick,
                enabled = enabled,
                shape = RoundedCornerShape(12.dp),
                border = BorderStroke(1.dp, MmSecondaryButtonOutline),
                colors = ButtonDefaults.outlinedButtonColors(
                    containerColor = MmSecondaryButtonBackground,
                    contentColor = MmTextPrimary,
                    disabledContainerColor = MmSecondaryButtonBackground,
                ),
            ) {
                MText(
                    text = cancelText,
                    color = MmTextPrimary,
                    fontWeight = FontWeight.Bold,
                )
            }

            Button(
                modifier = Modifier.weight(1f),
                onClick = onSaveClick,
                enabled = enabled,
                shape = RoundedCornerShape(12.dp),
                colors = ButtonDefaults.buttonColors(
                    containerColor = MmPrimary,
                    contentColor = MmTextPrimary,
                    disabledContainerColor = MmPrimary.copy(alpha = 0.5f),
                ),
            ) {
                MText(
                    text = saveText,
                    color = MmTextPrimary,
                    fontWeight = FontWeight.Bold,
                )
            }
        }
    }
}
