package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MModeButton
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.Res
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_locked_title
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_retry
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_sign_out
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.biometric_unlock
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.presentation.biometric.AppLockUiState
import org.jetbrains.compose.resources.stringResource

/**
 * Renders app lock screen from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun AppLockScreen(
    uiState: AppLockUiState,
    onUnlock: () -> Unit,
    onRetryInitialization: () -> Unit,
    onSignOut: () -> Unit
): Unit {
    Surface(
        modifier = Modifier.fillMaxSize(),
        color = MaterialTheme.colorScheme.background
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(all = 24.dp),
            verticalArrangement = Arrangement.spacedBy(
                space = 16.dp,
                alignment = Alignment.CenterVertically
            ),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            MText(
                text = stringResource(resource = Res.string.biometric_locked_title),
                color = MaterialTheme.colorScheme.onSurface
            )

            appLockErrorText(error = uiState.error)?.let { message ->
                MText(
                    text = message,
                    color = MaterialTheme.colorScheme.error
                )
            }

            if (uiState.isBusy) {
                CircularProgressIndicator()
            } else {
                MModeButton(
                    modifier = Modifier.fillMaxWidth(),
                    text = stringResource(
                        resource = if (uiState.isInitialized) {
                            Res.string.biometric_unlock
                        } else {
                            Res.string.biometric_retry
                        }
                    ),
                    selected = true,
                    onClick = if (uiState.isInitialized) {
                        onUnlock
                    } else {
                        onRetryInitialization
                    }
                )

                MModeButton(
                    modifier = Modifier.fillMaxWidth(),
                    text = stringResource(resource = Res.string.biometric_sign_out),
                    selected = false,
                    onClick = onSignOut
                )
            }
        }
    }
}
@Preview(showBackground = true)
@Composable
private fun AppLockScreenPreview() {
    MelodiasMarioTheme {
        AppLockScreen(
            uiState = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = true
            ),
            onUnlock = {},
            onRetryInitialization = {},
            onSignOut = {}
        )
    }
}
