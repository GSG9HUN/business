package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.foundation.layout.Column
import androidx.compose.material3.MaterialTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.components.settings.MToggleRow
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.*
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.presentation.biometric.AppLockUiState
import org.jetbrains.compose.resources.stringResource

@Composable
fun BiometricLockSetting(
    state: AppLockUiState,
    onCheckedChange: (Boolean) -> Unit,
) {
    Column {
        MToggleRow(
            icon = Res.drawable.ic_profile_lock,
            title = stringResource(Res.string.biometric_setting_title),
            subtitle = stringResource(Res.string.biometric_setting_subtitle),
            checked = state.isEnabled,
            onCheckedChange = { enabled ->
                if (state.isInitialized && !state.isBusy && !state.isLocked) {
                    onCheckedChange(enabled)
                }
            },
        )

        appLockErrorText(state.error)?.let { message ->
            MText(
                text = message,
                color = MaterialTheme.colorScheme.error,
            )
        }
    }
}

@Preview(showBackground = true)
@Composable
private fun BiometricLockSettingPreview() {
    MelodiasMarioTheme {
        BiometricLockSetting(
            state = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = false,
            ),
            onCheckedChange = {},
        )
    }
}
