package com.dc.melodiasmario.shared.presentation.auth

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.shared.AppConstants.PollingIntervalMs
import com.dc.melodiasmario.shared.core.Resource
import com.dc.melodiasmario.shared.domain.auth.usecase.ExchangeAuthTicketUseCase
import com.dc.melodiasmario.shared.domain.auth.usecase.StartDiscordLoginUseCase
import com.dc.melodiasmario.shared.domain.status.model.ApiConnectionStatus
import com.dc.melodiasmario.shared.domain.status.usecase.CheckApiStatusUseCase
import com.dc.melodiasmario.shared.presentation.auth.LoginEffect.*
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel
import kotlin.time.Duration.Companion.milliseconds

@KoinViewModel
class LoginViewModel(
    private val checkApiStatusUseCase: CheckApiStatusUseCase,
    private val startDiscordLoginUseCase: StartDiscordLoginUseCase,
    private val exchangeAuthTicketUseCase: ExchangeAuthTicketUseCase,
    private val authDeepLinkDispatcher: AuthDeepLinkDispatcher,
) : ViewModel() {
    private val _uiState = MutableStateFlow(LoginUiState())
    val uiState: StateFlow<LoginUiState> = _uiState.asStateFlow()
    private val _effect = MutableSharedFlow<LoginEffect>()
    val effect = _effect.asSharedFlow()
    private var pollingJob: Job? = null

    init {
        observeAuthTickets()
    }

    override fun onCleared() {
        stopStatusPolling()
        super.onCleared()
    }
    private fun observeAuthTickets() {
        viewModelScope.launch {
            authDeepLinkDispatcher.tickets.collect { ticket ->
                onEvent(LoginEvent.AuthTicketReceived(ticket))
            }
        }
    }

    fun onEvent(event: LoginEvent){
        when(event){
            LoginEvent.StartStatusPolling -> startStatusPolling()
            LoginEvent.StopStatusPolling -> stopStatusPolling()
            LoginEvent.DiscordLoginClicked -> {
                viewModelScope.launch {
                    startDiscordLoginUseCase().collect { result ->
                        when (result) {
                            is Resource.Loading -> {
                                _uiState.update { currentState ->
                                    currentState.copy(isLoading = true)
                                }
                            }
                            is Resource.Success -> {
                                _uiState.update { currentState ->
                                    currentState.copy(isLoading = false)
                                }
                                _effect.emit(OpenExternalUrl(result.data.authorizeUrl))
                            }
                            is Resource.Error -> {
                                _uiState.update { currentState ->
                                    currentState.copy(
                                        isLoading = false,
                                        errorMessage = result.error.message
                                    )
                                }
                                _effect.emit(ShowError(result.error.message ?: "Login failed"))
                            }
                        }
                    }
                }
            }

            is LoginEvent.AuthTicketReceived -> {
                viewModelScope.launch {
                    exchangeAuthTicketUseCase(event.ticket).collect { result ->
                        when (result) {
                            is Resource.Loading -> {
                                _uiState.update { currentState ->
                                    currentState.copy(isLoading = true)
                                }
                            }
                            is Resource.Success -> {
                                _uiState.update { currentState ->
                                    currentState.copy(isLoading = false)
                                }
                                stopStatusPolling()
                                _effect.emit(NavigateToGuildSelector)
                            }
                            is Resource.Error -> {
                                _uiState.update { currentState ->
                                    currentState.copy(
                                        isLoading = false,
                                        errorMessage = result.error.message
                                    )
                                }
                                _effect.emit(ShowError(result.error.message ?: "Login failed"))
                            }
                        }
                    }
                }
            }
        }
    }

    private fun startStatusPolling() {
        if (pollingJob?.isActive == true) {
            return
        }

        pollingJob = viewModelScope.launch {
            while (isActive) {
                checkApiStatusUseCase().collect { status ->
                    when (status) {
                        is Resource.Loading -> {
                            _uiState.update { currentState ->
                                currentState.copy(isLoading = true)
                            }
                        }
                        is Resource.Success -> {
                            _uiState.update { currentState ->
                                currentState.copy(
                                    isLoading = false,
                                    apiConnectionStatus = status.data
                                )
                            }
                        }
                        is Resource.Error -> {
                            _uiState.update { currentState ->
                                currentState.copy(
                                    apiConnectionStatus = ApiConnectionStatus.Offline,
                                    isLoading = false,
                                    errorMessage = status.error.message
                                )
                            }
                        }
                    }
                }
                delay(PollingIntervalMs.milliseconds)
            }
        }
    }
    private fun stopStatusPolling() {
        pollingJob?.cancel()
        pollingJob = null
    }
}
