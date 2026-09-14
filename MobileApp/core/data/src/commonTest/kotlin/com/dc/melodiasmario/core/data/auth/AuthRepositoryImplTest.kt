package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.AuthSession
import com.dc.melodiasmario.core.model.auth.DiscordLoginUrl
import com.dc.melodiasmario.core.network.auth.AuthRemoteDataSource
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import kotlin.test.assertEquals
import kotlin.test.assertFailsWith
import kotlin.test.assertNull
import kotlin.test.Test

class AuthRepositoryImplTest {

    @Test
    fun startDiscordLoginReturnsRemoteAuthorizeUrl(): TestResult = runTest {
        val remote = FakeAuthRemoteDataSource(
            loginUrl = DiscordLoginUrl(authorizeUrl = "https://discord.test/login")
        )
        val repository = AuthRepositoryImpl(remote, FakeSessionStorage())

        val result = repository.startDiscordLogin()

        assertEquals(DiscordLoginUrl("https://discord.test/login"), result)
    }

    @Test
    fun exchangeTicketSavesReturnedSession(): TestResult = runTest {
        val session =
            authSession(accessToken = "access-from-ticket", refreshToken = "refresh-from-ticket")
        val storage = FakeSessionStorage()
        val repository = AuthRepositoryImpl(
            authRemoteDataSource = FakeAuthRemoteDataSource(exchangeSession = session),
            secureAuthSessionStorage = storage
        )

        val result = repository.exchangeTicket(ticket = "ticket")

        assertEquals(session, result)
        assertEquals(session, storage.getSession())
    }

    @Test
    fun refreshSessionSavesReturnedSession(): TestResult = runTest {
        val session =
            authSession(accessToken = "refreshed-access", refreshToken = "refreshed-refresh")
        val storage = FakeSessionStorage()
        val repository = AuthRepositoryImpl(
            authRemoteDataSource = FakeAuthRemoteDataSource(refreshSession = session),
            secureAuthSessionStorage = storage
        )

        val result = repository.refreshSession(refreshToken = "old-refresh")

        assertEquals(session, result)
        assertEquals(session, storage.getSession())
    }

    @Test
    fun logoutCallsRemoteLogoutAndClearsStoredSession(): TestResult = runTest {
        val remote = FakeAuthRemoteDataSource()
        val storage = FakeSessionStorage(session = authSession())
        val repository = AuthRepositoryImpl(remote, storage)

        repository.logout(refreshToken = "refresh")

        assertEquals("refresh", remote.lastLogoutRefreshToken)
        assertNull(storage.getSession())
        assertEquals(1, storage.clearCalls)
    }

    @Test
    fun logoutDoesNotClearSessionWhenRemoteLogoutFails(): TestResult = runTest {
        val storedSession = authSession()
        val storage = FakeSessionStorage(session = storedSession)
        val repository = AuthRepositoryImpl(
            authRemoteDataSource = FakeAuthRemoteDataSource(
                logoutException = IllegalStateException(
                    "logout failed"
                )
            ),
            secureAuthSessionStorage = storage
        )

        val exception = assertFailsWith<IllegalStateException> {
            repository.logout(refreshToken = "refresh")
        }

        assertEquals("logout failed", exception.message)
        assertEquals(storedSession, storage.getSession())
        assertEquals(0, storage.clearCalls)
    }

    private class FakeAuthRemoteDataSource(
        private val loginUrl: DiscordLoginUrl = DiscordLoginUrl("https://discord.test/default"),
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
            logoutException?.let { throw it }
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
