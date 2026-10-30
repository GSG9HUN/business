package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.settings.MToggleRow
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_setting_subtitle
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_setting_title
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.ic_profile_lock
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.presentation.biometric.AppLockUiState
import org.jetbrains.compose.resources.stringResource

/**
 * Renders biometric lock setting from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun BiometricLockSetting(
    uiState: AppLockUiState,
    onCheckedChange: (checked: Boolean) -> Unit
): Unit {
    Column {
        MToggleRow(
            icon = Res.drawable.ic_profile_lock,
            title = stringResource(resource = Res.string.biometric_setting_title),
            subtitle = stringResource(resource = Res.string.biometric_setting_subtitle),
            checked = uiState.isEnabled,
            onCheckedChange = { enabled ->

                if (uiState.isInitialized && !uiState.isBusy && !uiState.isLocked) {
                    onCheckedChange(enabled)
                }
            }
        )

        appLockErrorText(error = uiState.error)?.let { message ->
            MText(
                text = message,
                color = MaterialTheme.colorScheme.error
            )
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun BiometricLockSettingPreview() {
    MelodiasMarioTheme {
        BiometricLockSetting(
            uiState = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = false
            ),
            onCheckedChange = {}
        )
    }
}
