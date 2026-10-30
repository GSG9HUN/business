package com.dc.melodiasmario.feature.login.ui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.collectAsState
import androidx.compose.runtime.getValue
import androidx.compose.ui.Modifier
import com.dc.melodiasmario.core.domain.auth.platform.ExternalUrlOpener
import com.dc.melodiasmario.feature.login.presentation.LoginEffect
import com.dc.melodiasmario.feature.login.presentation.LoginEvent
import com.dc.melodiasmario.feature.login.presentation.LoginViewModel
import org.koin.compose.koinInject
import org.koin.compose.viewmodel.koinViewModel

/**
 * Connects login presentation state and effects to its screen callbacks.
 */
@Composable
fun LoginRoute(
    modifier: Modifier = Modifier,
    loginViewModel: LoginViewModel = koinViewModel(),
    onLoginSuccess: () -> Unit = {},
    externalUrlOpenerImpl: ExternalUrlOpener = koinInject()
): Unit {
    val uiState by loginViewModel.uiState.collectAsState()

    LaunchedEffect(key1 = Unit) {
        loginViewModel.onEvent(event = LoginEvent.StartStatusPolling)
    }

    DisposableEffect(key1 = Unit) {
        onDispose {
            loginViewModel.onEvent(event = LoginEvent.StopStatusPolling)
        }
    }

    LaunchedEffect(key1 = Unit) {
        loginViewModel.effect.collect { effect ->
            when (effect) {
                is LoginEffect.OpenExternalUrl -> {
                    externalUrlOpenerImpl.openUrl(url = effect.url)
                }
                LoginEffect.NavigateToGuildSelector -> onLoginSuccess()
                is LoginEffect.ShowError -> {
                    //TODO error showing
                }

            }
        }
    }

    LoginScreen(
        uiState = uiState,
        modifier = modifier,
        onEvent = loginViewModel::onEvent
    )
}
