package com.dc.melodiasmario.core.presentation.biometric

import kotlinx.coroutines.CancellationException
import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.asStateFlow
import kotlinx.coroutines.flow.update
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import kotlin.time.Clock

class AppLockController(
    private val hasStoredSession: suspend () -> Boolean,
    private val loadEnabled: suspend () -> Boolean,
    private val saveEnabled: suspend (Boolean) -> Unit,
    private val clearLocalSession: suspend () -> Unit,
    private val backgroundTimeoutMillis: Long = 30_000L,
    private val nowMillis: () -> Long = { Clock.System.now().toEpochMilliseconds() },
) {
    private val _uiState = MutableStateFlow(AppLockUiState())
    val uiState = _uiState.asStateFlow()

    private val initializationMutex = Mutex()

    private var backgroundStartedAt: Long? = null
    private var lastAuthenticationAt: Long? = null

    suspend fun initialize() {
        initializationMutex.withLock {
            if (_uiState.value.isInitialized) {
                return@withLock
            }

            _uiState.update {
                it.copy(
                    isBusy = true,
                    error = null,
                )
            }

            try {
                val enabled = if (hasStoredSession()) {
                    loadEnabled()
                } else {
                    saveEnabled(false)
                    false
                }

                _uiState.update {
                    it.copy(
                        isInitialized = true,
                        isEnabled = enabled,
                        isLocked = enabled,
                    )
                }
            } catch (error: CancellationException) {
                throw error
            } catch (_: Exception) {
                _uiState.update {
                    it.copy(
                        isInitialized = false,
                        isLocked = true,
                        error = AppLockError.StorageFailure,
                    )
                }
            } finally {
                _uiState.update {
                    it.copy(isBusy = false)
                }
            }
        }
    }

    fun onBackground() {
        if (!_uiState.value.isForeground) {
            return
        }

        backgroundStartedAt = nowMillis()

        _uiState.update {
            it.copy(isForeground = false)
        }
    }

    fun onForeground() {
        if (_uiState.value.isForeground) {
            return
        }

        val now = nowMillis()
        val backgroundStart = backgroundStartedAt

        val timeoutExpired = backgroundStart?.let {
            val elapsed = now - it
            elapsed < 0L || elapsed >= backgroundTimeoutMillis
        } ?: false

        val authenticatedRecently = lastAuthenticationAt?.let {
            val elapsed = now - it

            backgroundStart != null && it >= backgroundStart && elapsed >= 0L && elapsed < backgroundTimeoutMillis
        } ?: false

        _uiState.update {
            it.copy(
                isForeground = true,
                isLocked = it.isLocked || (it.isEnabled && timeoutExpired && !authenticatedRecently),
            )
        }

        backgroundStartedAt = null
    }

    suspend fun unlock(
        authenticate: suspend () -> BiometricResult,
    ) {
        val state = _uiState.value

        if (!state.isEnabled || !state.isLocked) {
            return
        }

        authenticateAndRun(authenticate) {
            _uiState.update {
                it.copy(isLocked = false)
            }
        }
    }

    suspend fun changeEnabled(
        enabled: Boolean,
        authenticate: suspend () -> BiometricResult,
    ) {
        val state = _uiState.value

        if (state.isLocked || state.isEnabled == enabled) {
            return
        }

        authenticateAndRun(authenticate) {
            saveEnabled(enabled)

            _uiState.update {
                it.copy(
                    isEnabled = enabled,
                    isLocked = false,
                )
            }
        }
    }

    suspend fun signOut(): Boolean {
        if (_uiState.value.isBusy) {
            return false
        }

        _uiState.update {
            it.copy(
                isBusy = true,
                error = null,
            )
        }

        return try {
            clearLocalSession()
            saveEnabled(false)

            lastAuthenticationAt = null

            _uiState.update {
                it.copy(
                    isInitialized = true,
                    isEnabled = false,
                    isLocked = false,
                    error = null,
                )
            }

            true
        } catch (error: CancellationException) {
            throw error
        } catch (_: Exception) {
            _uiState.update {
                it.copy(error = AppLockError.StorageFailure)
            }

            false
        } finally {
            _uiState.update {
                it.copy(isBusy = false)
            }
        }
    }

    private suspend fun authenticateAndRun(
        authenticate: suspend () -> BiometricResult,
        onSuccess: suspend () -> Unit,
    ) {
        val state = _uiState.value

        if (!state.isInitialized || !state.isForeground || state.isBusy) {
            return
        }

        _uiState.update {
            it.copy(
                isBusy = true,
                error = null,
            )
        }

        try {
            when (authenticate()) {
                BiometricResult.Success -> {
                    lastAuthenticationAt = nowMillis()
                    onSuccess()
                }

                BiometricResult.NotAuthenticated -> {
                    _uiState.update {
                        it.copy(
                            error = AppLockError.NotAuthenticated,
                        )
                    }
                }

                BiometricResult.Unavailable -> {
                    _uiState.update {
                        it.copy(
                            error = AppLockError.AuthenticationUnavailable,
                        )
                    }
                }
            }
        } catch (error: CancellationException) {
            throw error
        } catch (_: Exception) {
            _uiState.update {
                it.copy(error = AppLockError.StorageFailure)
            }
        } finally {
            _uiState.update {
                it.copy(isBusy = false)
            }
        }
    }
}