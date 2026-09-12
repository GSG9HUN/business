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

@Composable
fun LoginRoute(
    modifier: Modifier = Modifier,
    viewModel: LoginViewModel = koinViewModel(),
    onLoginSuccess: () -> Unit = {},
    externalUrlOpenerImpl: ExternalUrlOpener = koinInject(),
) {
    val uiState by viewModel.uiState.collectAsState()

    LaunchedEffect(Unit) {
        viewModel.onEvent(LoginEvent.StartStatusPolling)
    }

    DisposableEffect(Unit) {
        onDispose {
            viewModel.onEvent(LoginEvent.StopStatusPolling)
        }
    }

    LaunchedEffect(Unit) {
        viewModel.effect.collect { effect ->
            when (effect) {
                is LoginEffect.OpenExternalUrl -> {
                    externalUrlOpenerImpl.openUrl(effect.url)
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
        onEvent = viewModel::onEvent,
    )
}
