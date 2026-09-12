package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.Test
import kotlin.time.Clock

class AuthorizedSessionProviderTest {

    @Test
    fun getValidSessionThrowsWhenNoSessionExists(): TestResult = runTest {
        val provider = AuthorizedSessionProvider(
            secureAuthSessionStorage = FakeSessionStorage(),
            authRepository = FakeAuthRepository()
        )

        val exception = assertFailsWith<Exception> {
            provider.getValidSession()
        }

        assertEquals("No session found", exception.message)
    }

    @Test
    fun getValidSessionReturnsCurrentAccessTokenWhenSessionIsStillValid(): TestResult = runTest {
        val repository = FakeAuthRepository()
        val provider = AuthorizedSessionProvider(
            secureAuthSessionStorage = FakeSessionStorage(
                session = authSession(
                    accessToken = "current-access",
                    refreshToken = "current-refresh",
                    expiresAtMillis = futureMillis()
                )
            ),
            authRepository = repository
        )

        val accessToken = provider.getValidSession()

        assertEquals("current-access", accessToken)
        assertEquals(0, repository.refreshCalls)
    }

    @Test
    fun getValidSessionRefreshesSessionWhenCurrentSessionIsExpiredOrCloseToExpiry(): TestResult =
        runTest {
            val repository = FakeAuthRepository(
                refreshedSession = authSession(
                    accessToken = "new-access",
                    refreshToken = "new-refresh"
                )
            )
            val provider = AuthorizedSessionProvider(
                secureAuthSessionStorage = FakeSessionStorage(
                    session = authSession(
                        accessToken = "old-access",
                        refreshToken = "old-refresh",
                        expiresAtMillis = expiredMillis()
                    )
                ),
                authRepository = repository
            )

            provider.getValidSession()

            assertEquals(1, repository.refreshCalls)
            assertEquals("old-refresh", repository.lastRefreshToken)
        }

    @Test
    fun getValidSessionReturnsRefreshedAccessTokenAfterRefresh(): TestResult = runTest {
        val provider = AuthorizedSessionProvider(
            secureAuthSessionStorage = FakeSessionStorage(
                session = authSession(
                    accessToken = "old-access",
                    refreshToken = "old-refresh",
                    expiresAtMillis = expiredMillis()
                )
            ),
            authRepository = FakeAuthRepository(
                refreshedSession = authSession(
                    accessToken = "new-access",
                    refreshToken = "new-refresh"
                )
            )
        )

        val accessToken = provider.getValidSession()

        assertEquals("new-access", accessToken)
    }

    private class FakeSessionStorage(
        private var session: AuthSession? = null
    ) : SecureAuthSessionStorage {
        override suspend fun saveSession(session: AuthSession) {
            this.session = session
        }

        override suspend fun clearSession() {
            session = null
        }

        override suspend fun getSession(): AuthSession? = session
    }

    private class FakeAuthRepository(
        private val refreshedSession: AuthSession = authSession(
            accessToken = "refreshed-access",
            refreshToken = "refreshed-refresh"
        )
    ) : AuthRepository {
        var refreshCalls = 0
            private set
        var lastRefreshToken: String? = null
            private set

        override suspend fun startDiscordLogin(): DiscordLoginUrl =
            error("Not used by this test")

        override suspend fun exchangeTicket(ticket: String): AuthSession =
            error("Not used by this test")

        override suspend fun refreshSession(refreshToken: String): AuthSession {
            refreshCalls++
            lastRefreshToken = refreshToken
            return refreshedSession
        }

        override suspend fun logout(refreshToken: String) {
            error("Not used by this test")
        }
    }

    private companion object {
        fun authSession(
            accessToken: String,
            refreshToken: String,
            expiresAtMillis: Long = futureMillis()
        ): AuthSession = AuthSession(
            accessToken = accessToken,
            refreshToken = refreshToken,
            expiresInSeconds = 3600,
            expiresAtMillis = expiresAtMillis
        )

        fun futureMillis(): Long = Clock.System.now().toEpochMilliseconds() + 120_000L

        fun expiredMillis(): Long = Clock.System.now().toEpochMilliseconds() - 1L
    }
}
