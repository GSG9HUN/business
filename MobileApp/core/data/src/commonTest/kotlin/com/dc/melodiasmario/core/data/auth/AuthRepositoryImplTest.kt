package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import com.dc.melodiasmario.core.network.auth.AuthRemoteDataSource
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNull
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest

class AuthRepositoryImplTest {

    @Test
    fun startDiscordLoginReturnsRemoteAuthorizeUrl(): TestResult = runTest {
        val fakeAuthRemoteDataSource = FakeAuthRemoteDataSource(
            loginUrl = DiscordLoginUrl(authorizeUrl = "https://discord.test/login")
        )
        val authRepositoryImpl = AuthRepositoryImpl(
            authRemoteDataSource = fakeAuthRemoteDataSource,
            secureAuthSessionStorage = FakeSessionStorage(),
            authSessionMemoryCache = AuthSessionMemoryCache()
        )

        val result = authRepositoryImpl.startDiscordLogin()

        assertEquals(expected = DiscordLoginUrl(authorizeUrl = "https://discord.test/login"), actual = result)
    }

    @Test
    fun exchangeTicketSavesReturnedSession(): TestResult = runTest {
        val session =
            authSession(accessToken = "access-from-ticket", refreshToken = "refresh-from-ticket")
        val fakeSessionStorage = FakeSessionStorage()
        val authRepositoryImpl = AuthRepositoryImpl(
            authRemoteDataSource = FakeAuthRemoteDataSource(exchangeSession = session),
            secureAuthSessionStorage = fakeSessionStorage,
            authSessionMemoryCache = AuthSessionMemoryCache()
        )

        val result = authRepositoryImpl.exchangeTicket(ticket = "ticket")

        assertEquals(expected = session, actual = result)
        assertEquals(expected = session, actual = fakeSessionStorage.getSession())
    }

    @Test
    fun refreshSessionSavesReturnedSession(): TestResult = runTest {
        val session =
            authSession(accessToken = "refreshed-access", refreshToken = "refreshed-refresh")
        val fakeSessionStorage = FakeSessionStorage()
        val authRepositoryImpl = AuthRepositoryImpl(
            authRemoteDataSource = FakeAuthRemoteDataSource(refreshSession = session),
            secureAuthSessionStorage = fakeSessionStorage,
            authSessionMemoryCache = AuthSessionMemoryCache()
        )

        val result = authRepositoryImpl.refreshSession(refreshToken = "old-refresh")

        assertEquals(expected = session, actual = result)
        assertEquals(expected = session, actual = fakeSessionStorage.getSession())
    }

    @Test
    fun logoutCallsRemoteLogoutAndClearsStoredSession(): TestResult = runTest {
        val fakeAuthRemoteDataSource = FakeAuthRemoteDataSource()
        val fakeSessionStorage = FakeSessionStorage(session = authSession())
        val authRepositoryImpl = AuthRepositoryImpl(
            authRemoteDataSource = fakeAuthRemoteDataSource,
            secureAuthSessionStorage = fakeSessionStorage,
            authSessionMemoryCache = AuthSessionMemoryCache()
        )

        authRepositoryImpl.logout(refreshToken = "refresh")

        assertEquals(expected = "refresh", actual = fakeAuthRemoteDataSource.lastLogoutRefreshToken)
        assertNull(actual = fakeSessionStorage.getSession())
        assertEquals(expected = 1, actual = fakeSessionStorage.clearCalls)
    }

    @Test
    fun logoutDoesNotClearSessionWhenRemoteLogoutFails(): TestResult = runTest {
        val storedSession = authSession()
        val fakeSessionStorage = FakeSessionStorage(session = storedSession)
        val authRepositoryImpl = AuthRepositoryImpl(
            authRemoteDataSource = FakeAuthRemoteDataSource(
                logoutException = IllegalStateException(
                    "logout failed"
                )
            ),
            secureAuthSessionStorage = fakeSessionStorage,
            authSessionMemoryCache = AuthSessionMemoryCache()
        )

        val exception = assertFailsWith<IllegalStateException> {
            authRepositoryImpl.logout(refreshToken = "refresh")
        }

        assertEquals(expected = "logout failed", actual = exception.message)
        assertEquals(expected = storedSession, actual = fakeSessionStorage.getSession())
        assertEquals(expected = 0, actual = fakeSessionStorage.clearCalls)
    }

    private class FakeAuthRemoteDataSource(
        private val loginUrl: DiscordLoginUrl = DiscordLoginUrl(authorizeUrl = "https://discord.test/default"),
        private val exchangeSession: AuthSession = authSession(
            accessToken = "exchange-access",
            refreshToken = "exchange-refresh"
        ),
        private val refreshSession: AuthSession = authSession(
            accessToken = "refresh-access",
            refreshToken = "refresh-refresh"
        ),
        private val logoutException: Throwable? = null
    ) : AuthRemoteDataSource {
        var lastLogoutRefreshToken: String? = null
            private set

        override suspend fun startDiscordLogin(): DiscordLoginUrl = loginUrl

        override suspend fun exchangeTicket(ticket: String): AuthSession = exchangeSession

        override suspend fun refreshSession(refreshToken: String): AuthSession = refreshSession

        override suspend fun logout(refreshToken: String) {
            lastLogoutRefreshToken = refreshToken
            logoutException?.let { logoutException -> throw logoutException }
        }
    }

    private class FakeSessionStorage(
        private var session: AuthSession? = null
    ) : SecureAuthSessionStorage {
        var clearCalls = 0
            private set

        override suspend fun saveSession(session: AuthSession) {
            this.session = session
        }

        override suspend fun clearSession() {
            clearCalls++
            session = null
        }

        override suspend fun getSession(): AuthSession? = session
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
