package com.dc.melodiasmario.core.commonui.biometric

import androidx.compose.runtime.Composable
import androidx.compose.runtime.CompositionLocalProvider
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.tooling.preview.Preview
import com.dc.melodiasmario.core.commonui.designsystem.theme.MelodiasMarioTheme
import com.dc.melodiasmario.core.presentation.biometric.AppLockController
import com.dc.melodiasmario.core.presentation.biometric.AppLockUiState
import kotlinx.coroutines.launch

@Composable
fun AppLockHost(
    controller: AppLockController,
    authenticator: BiometricAuthenticator,
    onSignedOut: () -> Unit,
    content: @Composable () -> Unit,
) {
    val state by controller.uiState.collectAsState()
    val scope = rememberCoroutineScope()
    val authenticate = rememberBiometricAuthentication(authenticator)

    BindAppLockLifecycle(controller)

    LaunchedEffect(controller) {
        controller.initialize()
    }

    LaunchedEffect(authenticator, state.isInitialized, state.isLocked) {
        if (state.isInitialized && state.isLocked) {
            controller.unlock(authenticate)
        }
    }

    CompositionLocalProvider(
        LocalAppLockController provides controller,
        LocalBiometricAuthenticator provides authenticator,
    ) {
        AppLockHostContent(
            state = state,
            onUnlock = {
                scope.launch {
                    controller.unlock(authenticate)
                }
            },
            onRetryInitialization = {
                scope.launch {
                    controller.initialize()
                }
            },
            onSignOut = {
                scope.launch {
                    if (controller.signOut()) {
                        onSignedOut()
                    }
                }
            },
            content = content,
        )
    }
}

@Composable
private fun AppLockHostContent(
    state: AppLockUiState,
    onUnlock: () -> Unit,
    onRetryInitialization: () -> Unit,
    onSignOut: () -> Unit,
    content: @Composable () -> Unit,
) {
    if (!state.isInitialized || state.isLocked) {
        AppLockScreen(
            state = state,
            onUnlock = onUnlock,
            onRetryInitialization = onRetryInitialization,
            onSignOut = onSignOut,
        )
    } else {
        content()
    }
}

@Preview(showBackground = true)
@Composable
private fun AppLockHostPreview() {
    MelodiasMarioTheme {
        AppLockHostContent(
            state = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = true,
            ),
            onUnlock = {},
            onRetryInitialization = {},
            onSignOut = {},
            content = {},
        )
    }
}
