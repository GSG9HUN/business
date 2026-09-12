package com.dc.melodiasmario.core.datastore.auth

import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.Test

class SecureAuthSessionStorageImplTest {

    @Test
    fun getSessionReturnsNullBeforeSessionIsSaved(): TestResult = runTest {
        val storage = SecureAuthSessionStorageImpl()

        assertNull(storage.getSession())
    }

    @Test
    fun saveSessionStoresSessionInMemory(): TestResult = runTest {
        val storage = SecureAuthSessionStorageImpl()
        val session = authSession(accessToken = "access", refreshToken = "refresh")

        storage.saveSession(session)

        assertEquals(session, storage.getSession())
    }

    @Test
    fun saveSessionOverwritesExistingSession(): TestResult = runTest {
        val storage = SecureAuthSessionStorageImpl()
        val oldSession = authSession(accessToken = "old-access", refreshToken = "old-refresh")
        val newSession = authSession(accessToken = "new-access", refreshToken = "new-refresh")

        storage.saveSession(oldSession)
        storage.saveSession(newSession)

        assertEquals(newSession, storage.getSession())
    }

    @Test
    fun clearSessionRemovesStoredSession(): TestResult = runTest {
        val storage = SecureAuthSessionStorageImpl()
        storage.saveSession(authSession())

        storage.clearSession()

        assertNull(storage.getSession())
    }

    private fun authSession(
        accessToken: String = "access",
        refreshToken: String = "refresh"
    ): AuthSession = AuthSession(
        accessToken = accessToken,
        refreshToken = refreshToken,
        expiresInSeconds = 3600,
        expiresAtMillis = 123_456L
    )
}
