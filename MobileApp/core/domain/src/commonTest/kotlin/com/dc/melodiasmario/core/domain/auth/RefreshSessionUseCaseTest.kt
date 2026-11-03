package com.dc.melodiasmario.core.domain.auth

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.usecase.RefreshSessionUseCase
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlinx.coroutines.flow.toList
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest

class RefreshSessionUseCaseTest {

    @Test
    fun invokeEmitsLoadingThenSuccessWhenRefreshTokenExists(): TestResult = runTest {
        val refreshedSession = authSession(accessToken = "new-access", refreshToken = "new-refresh")
        val refreshSessionUseCase = RefreshSessionUseCase(
            authRepository = FakeAuthRepository(refreshedSession = refreshedSession),
            secureAuthSessionStorage = FakeSessionStorage(session = authSession(refreshToken = "stored-refresh"))
        )

        val emissions = refreshSessionUseCase().toList()

        assertEquals(expected = Resource.Loading, actual = emissions[0])
        assertEquals(expected = Resource.Success(data = refreshedSession), actual = emissions[1])
    }

    @Test
    fun invokeCallsRepositoryWithStoredRefreshToken(): TestResult = runTest {
        val fakeAuthRepository = FakeAuthRepository()
        val refreshSessionUseCase = RefreshSessionUseCase(
            authRepository = fakeAuthRepository,
            secureAuthSessionStorage = FakeSessionStorage(session = authSession(refreshToken = "stored-refresh"))
        )

        refreshSessionUseCase().toList()

        assertEquals(expected = "stored-refresh", actual = fakeAuthRepository.lastRefreshToken)
    }

    @Test
    fun invokeEmitsLoadingThenErrorWhenNoSessionExists(): TestResult = runTest {
        val refreshSessionUseCase = RefreshSessionUseCase(
            authRepository = FakeAuthRepository(),
            secureAuthSessionStorage = FakeSessionStorage()
        )

        val emissions = refreshSessionUseCase().toList()

        assertEquals(expected = Resource.Loading, actual = emissions[0])
        val error = assertIs<Resource.Error>(value = emissions[1])
        assertEquals(expected = "No refresh token found", actual = error.error.message)
    }

    @Test
    fun invokeEmitsLoadingThenErrorWhenRepositoryRefreshFails(): TestResult = runTest {
        val refreshSessionUseCase = RefreshSessionUseCase(
            authRepository = FakeAuthRepository(refreshException = IllegalStateException("refresh failed")),
            secureAuthSessionStorage = FakeSessionStorage(session = authSession(refreshToken = "stored-refresh"))
        )

        val emissions = refreshSessionUseCase().toList()

        assertEquals(expected = Resource.Loading, actual = emissions[0])
        val error = assertIs<Resource.Error>(value = emissions[1])
        assertEquals(expected = "refresh failed", actual = error.error.message)
    }

    private class FakeSessionStorage(
        private val session: AuthSession? = null
    ) : SecureAuthSessionStorage {
        override suspend fun saveSession(session: AuthSession) {
            error(message = "Not used by this test")
        }

        override suspend fun clearSession() {
            error(message = "Not used by this test")
        }

        override suspend fun getSession(): AuthSession? = session
    }

    private class FakeAuthRepository(
        private val refreshedSession: AuthSession = authSession(
            accessToken = "refreshed-access",
            refreshToken = "refreshed-refresh"
        ),
        private val refreshException: Throwable? = null
    ) : AuthRepository {
        var lastRefreshToken: String? = null
            private set

        override suspend fun startDiscordLogin(): DiscordLoginUrl =
            error(message = "Not used by this test")

        override suspend fun exchangeTicket(ticket: String): AuthSession =
            error(message = "Not used by this test")

        override suspend fun refreshSession(refreshToken: String): AuthSession {
            lastRefreshToken = refreshToken
            refreshException?.let { refreshException -> throw refreshException }
            return refreshedSession
        }

        override suspend fun logout(refreshToken: String) {
            error(message = "Not used by this test")
        }
    }

    private companion object {
        fun authSession(
            accessToken: String = "access",
            refreshToken: String = "refresh"
        ): AuthSession = AuthSession(
            accessToken = accessToken,
            refreshToken = refreshToken,
            expiresInSeconds = 3600,
            expiresAtMillis = 123_456L
        )
    }
}
