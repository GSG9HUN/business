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

/**
 * Renders app lock host from the supplied parameters and forwards interaction callbacks.
 */
@Composable
fun AppLockHost(
    appLockController: AppLockController,
    biometricAuthenticator: BiometricAuthenticator,
    onSignedOut: () -> Unit,
    content: @Composable () -> Unit
): Unit {
    val state by appLockController.uiState.collectAsState()
    val scope = rememberCoroutineScope()
    val authenticate = rememberBiometricAuthentication(biometricAuthenticator = biometricAuthenticator)

    BindAppLockLifecycle(appLockController = appLockController)

    LaunchedEffect(key1 = appLockController) {
        appLockController.initialize()
    }

    LaunchedEffect(key1 = biometricAuthenticator, key2 = state.isInitialized, key3 = state.isLocked) {
        if (state.isInitialized && state.isLocked) {
            appLockController.unlock(authenticate = authenticate)
        }
    }

    CompositionLocalProvider(
        localAppLockController provides appLockController,
        localBiometricAuthenticator provides biometricAuthenticator
    ) {
        AppLockHostContent(
            uiState = state,
            onUnlock = {
                scope.launch {
                    appLockController.unlock(authenticate = authenticate)
                }
            },
            onRetryInitialization = {
                scope.launch {
                    appLockController.initialize()
                }
            },
            onSignOut = {
                scope.launch {
                    if (appLockController.signOut()) {
                        onSignedOut()
                    }
                }
            },
            content = content
        )
    }
}

@Composable
private fun AppLockHostContent(
    uiState: AppLockUiState,
    onUnlock: () -> Unit,
    onRetryInitialization: () -> Unit,
    onSignOut: () -> Unit,
    content: @Composable () -> Unit
) {
    if (!uiState.isInitialized || uiState.isLocked) {
        AppLockScreen(
            uiState = uiState,
            onUnlock = onUnlock,
            onRetryInitialization = onRetryInitialization,
            onSignOut = onSignOut
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
            uiState = AppLockUiState(
                isInitialized = true,
                isEnabled = true,
                isLocked = true
            ),
            onUnlock = {},
            onRetryInitialization = {},
            onSignOut = {},
            content = {}
        )
    }
}
