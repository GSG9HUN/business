package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.foundation.layout.Arrangement
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import androidx.compose.runtime.Composable
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.dc.melodiasmario.core.commonui.designsystem.components.button.MModeButton
import com.dc.melodiasmario.core.commonui.designsystem.components.display.MText
import com.dc.melodiasmario.core.commonui.designsystem.generated.resources.*
import com.dc.melodiasmario.core.presentation.biometric.AppLockUiState
import org.jetbrains.compose.resources.stringResource

@Composable
fun AppLockScreen(
    state: AppLockUiState,
    onUnlock: () -> Unit,
    onRetryInitialization: () -> Unit,
    onSignOut: () -> Unit,
) {
    Surface(
        modifier = Modifier.fillMaxSize(),
        color = MaterialTheme.colorScheme.background,
    ) {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(24.dp),
            verticalArrangement = Arrangement.spacedBy(
                16.dp,
                Alignment.CenterVertically,
            ),
            horizontalAlignment = Alignment.CenterHorizontally,
        ) {
            MText(
                text = stringResource(Res.string.biometric_locked_title),
                color = MaterialTheme.colorScheme.onSurface,
            )

            appLockErrorText(state.error)?.let { message ->
                MText(
                    text = message,
                    color = MaterialTheme.colorScheme.error,
                )
            }

            if (state.isBusy) {
                CircularProgressIndicator()
            } else {
                MModeButton(
                    modifier = Modifier.fillMaxWidth(),
                    text = stringResource(
                        if (state.isInitialized) {
                            Res.string.biometric_unlock
                        } else {
                            Res.string.biometric_retry
                        }
                    ),
                    selected = true,
                    onClick = if (state.isInitialized) {
                        onUnlock
                    } else {
                        onRetryInitialization
                    },
                )

                MModeButton(
                    modifier = Modifier.fillMaxWidth(),
                    text = stringResource(Res.string.biometric_sign_out),
                    selected = false,
                    onClick = onSignOut,
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
            state = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = true,
            ),
            onUnlock = {},
            onRetryInitialization = {},
            onSignOut = {},
        )
    }
}
