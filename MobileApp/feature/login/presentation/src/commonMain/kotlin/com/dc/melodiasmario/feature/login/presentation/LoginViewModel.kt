package com.dc.melodiasmario.feature.login.presentation

import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewModelScope
import com.dc.melodiasmario.core.common.AppConstants.PollingIntervalMs
import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.usecase.AuthDeepLinkDispatcher
import com.dc.melodiasmario.core.domain.auth.usecase.ExchangeAuthTicketUseCase
import com.dc.melodiasmario.core.domain.auth.usecase.StartDiscordLoginUseCase
import com.dc.melodiasmario.core.domain.login.usecase.CheckApiStatusUseCase
import com.dc.melodiasmario.core.model.login.ApiConnectionStatus
import com.dc.melodiasmario.feature.login.presentation.LoginEffect.NavigateToGuildSelector
import com.dc.melodiasmario.feature.login.presentation.LoginEffect.OpenExternalUrl
import com.dc.melodiasmario.feature.login.presentation.LoginEffect.ShowError
import kotlin.time.Duration.Companion.milliseconds
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.SharingStarted
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asSharedFlow
import kotlinx.coroutines.flow.onStart
import kotlinx.coroutines.flow.stateIn
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.isActive
import kotlinx.coroutines.launch
import org.koin.core.annotation.KoinViewModel

/**
 * Coordinates login events, observable UI state, and one-time effects.
 */
@KoinViewModel
class LoginViewModel(
    private val checkApiStatusUseCase: CheckApiStatusUseCase,
    private val startDiscordLoginUseCase: StartDiscordLoginUseCase,
    private val exchangeAuthTicketUseCase: ExchangeAuthTicketUseCase,
    private val authDeepLinkDispatcher: AuthDeepLinkDispatcher
) : ViewModel() {
    private val _uiState = MutableStateFlow(value = LoginUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<LoginUiState> = _uiState.onStart {
        observeAuthTickets()
    }
        .stateIn(
            scope = viewModelScope,
            started = SharingStarted.WhileSubscribed(stopTimeoutMillis = 5_000),
            initialValue = LoginUiState()
        )
    private val _effect = MutableSharedFlow<LoginEffect>()

    /**
     * One-time presentation outcomes observed by UI consumers.
     */
    val effect: SharedFlow<LoginEffect> = _effect.asSharedFlow()
    private val events = MutableSharedFlow<LoginEvent>(extraBufferCapacity = 64)
    private var pollingJob: Job? = null

    init {
        collectEvents()
    }

    override fun onCleared(): Unit {
        stopStatusPolling()
        super.onCleared()
    }

    /**
     * Submits a user or lifecycle event to the feature event-processing flow.
     */
    fun onEvent(event: LoginEvent): Unit {
        viewModelScope.launch {
            events.emit(value = event)
        }
    }

    private fun collectEvents() {
        viewModelScope.launch {
            events.collect { event ->
                handleEvent(event = event)
            }
        }
    }

    private suspend fun handleEvent(event: LoginEvent) {
        when (event) {
            LoginEvent.StartStatusPolling -> startStatusPolling()
            LoginEvent.StopStatusPolling -> stopStatusPolling()
            LoginEvent.DiscordLoginClicked -> onDiscordLoginClicked()
            is LoginEvent.AuthTicketReceived -> onAuthTicketReceived(ticket = event.ticket)
        }
    }

    private fun observeAuthTickets() {
        viewModelScope.launch {
            authDeepLinkDispatcher.tickets.collect { ticket ->
                onEvent(event = LoginEvent.AuthTicketReceived(ticket = ticket))
            }
        }
    }

    private suspend fun onDiscordLoginClicked() {
        startDiscordLoginUseCase().collect { result ->
            when (result) {
                is Resource.Loading -> setLoading()

                is Resource.Success -> onDiscordLoginClickedSuccess(authorizeUrl = result.data.authorizeUrl)

                is Resource.Error -> setError(error = result.error)
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
                delay(duration = PollingIntervalMs.milliseconds)
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
        exchangeAuthTicketUseCase(ticket = ticket).collect { result ->
            when (result) {
                is Resource.Loading -> setLoading()
                is Resource.Success -> authTicketReceivedSuccess()

                is Resource.Error -> setError(error = result.error)
            }
        }
    }

    private suspend fun onDiscordLoginClickedSuccess(authorizeUrl: String) {
        _uiState.update { currentState ->
            currentState.copy(isLoading = false)
        }
        _effect.emit(value = OpenExternalUrl(url = authorizeUrl))
    }

    private suspend fun authTicketReceivedSuccess() {
        _uiState.update { currentState ->
            currentState.copy(isLoading = false)
        }
        stopStatusPolling()
        _effect.emit(value = NavigateToGuildSelector)
    }

    private suspend fun setError(error: Throwable) {
        _uiState.update { currentState ->
            currentState.copy(
                isLoading = false,
                errorMessage = error.message
            )
        }
        _effect.emit(value = ShowError(error = error.message ?: "Login failed"))
    }

    private fun setLoading() {
        _uiState.update { currentState ->
            currentState.copy(isLoading = true)
        }
    }
}
