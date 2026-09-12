package com.dc.melodiasmario.core.domain.auth

import com.dc.melodiasmario.core.common.Resource
import com.dc.melodiasmario.core.domain.auth.usecase.RefreshSessionUseCase
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import kotlinx.coroutines.flow.toList
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import kotlin.test.assertEquals
import kotlin.test.assertIs
import kotlin.test.Test

class RefreshSessionUseCaseTest {

    @Test
    fun invokeEmitsLoadingThenSuccessWhenRefreshTokenExists(): TestResult = runTest {
        val refreshedSession = authSession(accessToken = "new-access", refreshToken = "new-refresh")
        val useCase = RefreshSessionUseCase(
            authRepository = FakeAuthRepository(refreshedSession = refreshedSession),
            secureAuthSessionStorage = FakeSessionStorage(session = authSession(refreshToken = "stored-refresh"))
        )

        val emissions = useCase().toList()

        assertEquals(Resource.Loading, emissions[0])
        assertEquals(Resource.Success(refreshedSession), emissions[1])
    }

    @Test
    fun invokeCallsRepositoryWithStoredRefreshToken(): TestResult = runTest {
        val repository = FakeAuthRepository()
        val useCase = RefreshSessionUseCase(
            authRepository = repository,
            secureAuthSessionStorage = FakeSessionStorage(session = authSession(refreshToken = "stored-refresh"))
        )

        useCase().toList()

        assertEquals("stored-refresh", repository.lastRefreshToken)
    }

    @Test
    fun invokeEmitsLoadingThenErrorWhenNoSessionExists(): TestResult = runTest {
        val useCase = RefreshSessionUseCase(
            authRepository = FakeAuthRepository(),
            secureAuthSessionStorage = FakeSessionStorage()
        )

        val emissions = useCase().toList()

        assertEquals(Resource.Loading, emissions[0])
        val error = assertIs<Resource.Error>(emissions[1])
        assertEquals("No refresh token found", error.error.message)
    }

    @Test
    fun invokeEmitsLoadingThenErrorWhenRepositoryRefreshFails(): TestResult = runTest {
        val useCase = RefreshSessionUseCase(
            authRepository = FakeAuthRepository(refreshException = IllegalStateException("refresh failed")),
            secureAuthSessionStorage = FakeSessionStorage(session = authSession(refreshToken = "stored-refresh"))
        )

        val emissions = useCase().toList()

        assertEquals(Resource.Loading, emissions[0])
        val error = assertIs<Resource.Error>(emissions[1])
        assertEquals("refresh failed", error.error.message)
    }

    private class FakeSessionStorage(
        private val session: AuthSession? = null
    ) : SecureAuthSessionStorage {
        override suspend fun saveSession(session: AuthSession) {
            error("Not used by this test")
        }

        override suspend fun clearSession() {
            error("Not used by this test")
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
            error("Not used by this test")

        override suspend fun exchangeTicket(ticket: String): AuthSession =
            error("Not used by this test")

        override suspend fun refreshSession(refreshToken: String): AuthSession {
            lastRefreshToken = refreshToken
            refreshException?.let { throw it }
            return refreshedSession
        }

        override suspend fun logout(refreshToken: String) {
            error("Not used by this test")
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
