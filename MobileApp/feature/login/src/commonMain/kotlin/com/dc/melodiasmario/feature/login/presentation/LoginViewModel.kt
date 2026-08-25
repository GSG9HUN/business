package com.dc.melodiasmario.feature.login.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.AppConstants.PollingIntervalMs
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.auth.domain.usecase.ExchangeAuthTicketUseCase
import com.dc.melodiasmario.core.auth.domain.usecase.StartDiscordLoginUseCase
import com.dc.melodiasmario.core.auth.presentation.AuthDeepLinkDispatcher
import com.dc.melodiasmario.core.network.status.domain.model.ApiConnectionStatus
import com.dc.melodiasmario.core.network.status.domain.usecase.CheckApiStatusUseCase
import com.dc.melodiasmario.feature.login.presentation.LoginEffect.*
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
    private val events = MutableSharedFlow<LoginEvent>(extraBufferCapacity = 64)
    private var pollingJob: Job? = null

    init {
        observeAuthTickets()
        collectEvents()
    }

    override fun onCleared() {
        stopStatusPolling()
        super.onCleared()
    }

    fun onEvent(event: LoginEvent) {
        viewModelScope.launch {
            events.emit(event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event)
            }
        }
    }

    private suspend fun handleEvent(event: LoginEvent) {
        when (event) {
            LoginEvent.StartStatusPolling -> startStatusPolling()
            LoginEvent.StopStatusPolling -> stopStatusPolling()
            LoginEvent.DiscordLoginClicked -> onDiscordLoginClicked()
            is LoginEvent.AuthTicketReceived -> onAuthTicketReceived(event.ticket)
        }
    }

    private fun observeAuthTickets() {
        viewModelScope.launch {
            authDeepLinkDispatcher.tickets.collect { ticket ->
                onEvent(LoginEvent.AuthTicketReceived(ticket))
            }
        }
    }
    private suspend fun onDiscordLoginClicked() {
        startDiscordLoginUseCase().collect { result ->
            when (result) {
                is Resource.Loading -> setLoading()

                is Resource.Success -> onDiscordLoginClickedSuccess(result.data.authorizeUrl)

                is Resource.Error -> setError(result.error)
            }
        }
    }

    private fun startStatusPolling() {
        if (pollingJob?.isActive == true) {
            return
        }

        pollingJob = viewModelScope.launch {
            while (isActive) {
                checkApiStatus()
                delay(PollingIntervalMs.milliseconds)
            }
        }
    }

    private fun stopStatusPolling() {
        pollingJob?.cancel()
        pollingJob = null
    }

    private suspend fun checkApiStatus() {
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
    }

    private suspend fun onAuthTicketReceived(ticket: String) {
        exchangeAuthTicketUseCase(ticket).collect { result ->
            when (result) {
                is Resource.Loading -> setLoading()
                is Resource.Success -> authTicketReceivedSuccess()

                is Resource.Error -> setError(result.error)
            }
        }
    }

    private suspend fun onDiscordLoginClickedSuccess(authorizeUrl: String) {
        _uiState.update { currentState ->
            currentState.copy(isLoading = false)
        }
        _effect.emit(OpenExternalUrl(authorizeUrl))
    }

    private suspend fun authTicketReceivedSuccess() {
        _uiState.update { currentState ->
            currentState.copy(isLoading = false)
        }
        stopStatusPolling()
        _effect.emit(NavigateToGuildSelector)
    }

    private suspend fun setError(error: Throwable) {
        _uiState.update { currentState ->
            currentState.copy(
                isLoading = false,
                errorMessage = error.message
            )
        }
        _effect.emit(ShowError(error.message ?: "Login failed"))
    }

    private fun setLoading() {
        _uiState.update { currentState ->
            currentState.copy(isLoading = true)
        }
    }
}
