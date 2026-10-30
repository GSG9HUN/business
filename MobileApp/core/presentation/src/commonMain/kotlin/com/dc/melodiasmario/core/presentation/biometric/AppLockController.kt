package com.dc.melodiasmario.core.presentation.biometric

import kotlin.time.Clock
import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock

/**
 * Owns app lock coordination and its lifecycle-bound operations.
 */
class AppLockController(
    private val hasStoredSession: suspend () -> Boolean,
    private val loadEnabled: suspend () -> Boolean,
    private val saveEnabled: suspend (enabled: Boolean) -> Unit,
    private val clearLocalSession: suspend () -> Unit,
    private val backgroundTimeoutMillis: Long = 30_000L,
    private val nowMillis: () -> Long = { Clock.System.now()
        .toEpochMilliseconds() }
) {
    private val _uiState = MutableStateFlow(value = AppLockUiState())

    /**
     * Read-only presentation state observed by UI consumers.
     */
    val uiState: StateFlow<AppLockUiState> = _uiState.asStateFlow()

    private val initializationMutex = Mutex()

    private var backgroundStartedAt: Long? = null
    private var lastAuthenticationAt: Long? = null

    /**
     * Restores the application-lock settings and initializes its presentation state.
     */
    suspend fun initialize(): Unit {
        initializationMutex.withLock {
            if (_uiState.value.isInitialized) {
                return@withLock
            }

            _uiState.update { currentUiState ->
                currentUiState.copy(
                    isBusy = true,
                    error = null
                )
            }

            try {
                val enabled = if (hasStoredSession()) {
                    loadEnabled()
                } else {
                    saveEnabled(false)
                    false
                }

                _uiState.update { currentUiState ->
                    currentUiState.copy(
                        isInitialized = true,
                        isEnabled = enabled,
                        isLocked = enabled
                    )
                }
            } catch (error: CancellationException) {
                throw error
            } catch (_: Exception) {
                _uiState.update { currentUiState ->
                    currentUiState.copy(
                        isInitialized = false,
                        isLocked = true,
                        error = AppLockError.StorageFailure
                    )
                }
            } finally {
                _uiState.update { currentUiState ->
                    currentUiState.copy(isBusy = false)
                }
            }
        }
    }

    /**
     * Records when the application leaves the foreground for lock-timeout handling.
     */
    fun onBackground(): Unit {
        if (!_uiState.value.isForeground) {
            return
        }

        backgroundStartedAt = nowMillis()

        _uiState.update { currentUiState ->
            currentUiState.copy(isForeground = false)
        }
    }

    /**
     * Reevaluates the application lock when the application returns to the foreground.
     */
    fun onForeground(): Unit {
        if (_uiState.value.isForeground) {
            return
        }

        val now = nowMillis()
        val backgroundStart = backgroundStartedAt

        val timeoutExpired = backgroundStart?.let { backgroundStart ->
            val elapsed = now - backgroundStart
            elapsed !in 0L..<backgroundTimeoutMillis
        } ?: false

        val authenticatedRecently = lastAuthenticationAt?.let { lastAuthenticationAt ->
            val elapsed = now - lastAuthenticationAt

            backgroundStart != null && lastAuthenticationAt >= backgroundStart && elapsed in 0L..<backgroundTimeoutMillis
        } ?: false

        _uiState.update { currentUiState ->
            currentUiState.copy(
                isForeground = true,
                isLocked = currentUiState.isLocked || (currentUiState.isEnabled && timeoutExpired && !authenticatedRecently)
            )
        }

        backgroundStartedAt = null
    }

    /**
     * Authenticates the user before releasing the application lock.
     */
    suspend fun unlock(authenticate: suspend () -> BiometricResult): Unit {
        val currentUiState = _uiState.value

        if (!currentUiState.isEnabled || !currentUiState.isLocked) {
            return
        }

        authenticateAndRun(authenticate = authenticate) {
            _uiState.update { currentUiState ->
                currentUiState.copy(isLocked = false)
            }
        }
    }

    /**
     * Authenticates and persists a change to application-lock enablement.
     */
    suspend fun changeEnabled(
        enabled: Boolean,
        authenticate: suspend () -> BiometricResult
    ): Unit {
        val currentUiState = _uiState.value

        if (currentUiState.isLocked || currentUiState.isEnabled == enabled) {
            return
        }

        authenticateAndRun(authenticate = authenticate) {
            saveEnabled(enabled)

            _uiState.update { currentUiState ->
                currentUiState.copy(
                    isEnabled = enabled,
                    isLocked = false
                )
            }
        }
    }

    /**
     * Clears the active authentication session and application-lock state.
     */
    suspend fun signOut(): Boolean {
        if (_uiState.value.isBusy) {
            return false
        }

        _uiState.update { currentUiState ->
            currentUiState.copy(
                isBusy = true,
                error = null
            )
        }

        return try {
            clearLocalSession()
            saveEnabled(false)

            lastAuthenticationAt = null

            _uiState.update { currentUiState ->
                currentUiState.copy(
                    isInitialized = true,
                    isEnabled = false,
                    isLocked = false,
                    error = null
                )
            }

            true
        } catch (error: CancellationException) {
            throw error
        } catch (_: Exception) {
            _uiState.update { currentUiState ->
                currentUiState.copy(error = AppLockError.StorageFailure)
            }

            false
        } finally {
            _uiState.update { currentUiState ->
                currentUiState.copy(isBusy = false)
            }
        }
    }

    private suspend fun authenticateAndRun(
        authenticate: suspend () -> BiometricResult,
        onSuccess: suspend () -> Unit
    ) {
        val currentUiState = _uiState.value

        if (!currentUiState.isInitialized
            || !currentUiState.isForeground
            || currentUiState.isBusy) {
            return
        }

        _uiState.update { currentUiState ->
            currentUiState.copy(
                isBusy = true,
                error = null
            )
        }

        try {
            when (authenticate()) {
                BiometricResult.Success -> {
                    lastAuthenticationAt = nowMillis()
                    onSuccess()
                }

                BiometricResult.NotAuthenticated -> {
                    _uiState.update { currentUiState ->
                        currentUiState.copy(
                            error = AppLockError.NotAuthenticated
                        )
                    }
                }

                BiometricResult.Unavailable -> {
                    _uiState.update { currentUiState ->
                        currentUiState.copy(
                            error = AppLockError.AuthenticationUnavailable
                        )
                    }
                }
            }
        } catch (error: CancellationException) {
            throw error
        } catch (_: Exception) {
            _uiState.update { currentUiState ->
                currentUiState.copy(error = AppLockError.StorageFailure)
            }
        } finally {
            _uiState.update { currentUiState ->
                currentUiState.copy(isBusy = false)
            }
        }
    }
}
