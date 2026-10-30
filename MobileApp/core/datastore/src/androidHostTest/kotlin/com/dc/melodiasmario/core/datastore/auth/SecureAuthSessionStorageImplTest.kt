package com.dc.melodiasmario.core.datastore.auth

import android.content.Context
import androidx.datastore.core.DataStore
import androidx.datastore.preferences.core.PreferenceDataStoreFactory
import androidx.datastore.preferences.core.Preferences
import androidx.test.core.app.ApplicationProvider
import com.dc.melodiasmario.core.model.auth.AuthSession
import java.io.File
import java.util.UUID
import kotlin.test.Test
import kotlin.test.assertEquals
import kotlin.test.assertNull
import kotlin.test.assertTrue
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.SupervisorJob
import kotlinx.coroutines.flow.first
import kotlinx.coroutines.test.TestResult
import kotlinx.coroutines.test.runTest
import org.junit.runner.RunWith
import org.robolectric.RobolectricTestRunner

@RunWith(RobolectricTestRunner::class)
class SecureAuthSessionStorageImplTest {

    @Test
    fun getSessionReturnsNullBeforeSessionIsSaved(): TestResult = runTest {
        val storage = createStorage()

        assertNull(actual = storage.getSession())
    }

    @Test
    fun saveSessionStoresSessionInDataStore(): TestResult = runTest {
        val storage = createStorage()
        val session = authSession(accessToken = "access", refreshToken = "refresh")

        storage.saveSession(session = session)

        assertEquals(expected = session, actual = storage.getSession())
    }

    @Test
    fun saveSessionWritesEncryptedPayloadToDataStore(): TestResult = runTest {
        val context = ApplicationProvider.getApplicationContext<Context>()
        val dataStore = createTestDataStore(context = context)
        val storage = createStorage(context = context, dataStore = dataStore)
        val session = authSession(
            accessToken = "encrypted-access",
            refreshToken = "encrypted-refresh"
        )

        storage.saveSession(session = session)

        val storedValue = dataStore.data.first()
            .asMap()
            .values
            .single()
            .toString()

        assertTrue(actual = storedValue.startsWith(TestAuthSessionCipher.PREFIX))
    }

    @Test
    fun saveSessionPersistsAcrossStorageInstances(): TestResult = runTest {
        val context = ApplicationProvider.getApplicationContext<Context>()
        val dataStore = createTestDataStore(context = context)
        val storage = createStorage(context = context, dataStore = dataStore)
        val session = authSession(
            accessToken = "persisted-access",
            refreshToken = "persisted-refresh"
        )

        storage.saveSession(session = session)

        val restoredStorage = createStorage(context = context, dataStore = dataStore)

        assertEquals(expected = session, actual = restoredStorage.getSession())
    }

    @Test
    fun clearSessionKeepsEmptyDataStoreReadable(): TestResult = runTest {
        val storage = createStorage()

        storage.clearSession()

        assertNull(actual = storage.getSession())
    }

    private suspend fun createStorage(): SecureAuthSessionStorageImpl {
        val context = ApplicationProvider.getApplicationContext<Context>()
        return createStorage(context = context, dataStore = createTestDataStore(context = context)).also { storage ->
            storage.clearSession()
        }
    }

    private fun createStorage(
        context: Context,
        dataStore: DataStore<Preferences>
    ): SecureAuthSessionStorageImpl {
        return SecureAuthSessionStorageImpl(
            context = context,
            cipher = TestAuthSessionCipher,
            dataStore = dataStore
        )
    }

    private fun createTestDataStore(
        context: Context
    ): DataStore<Preferences> {
        val fileName = "secure_auth_session_test_${UUID.randomUUID()}.preferences_pb"

        return PreferenceDataStoreFactory.create(
            scope = CoroutineScope(context = Dispatchers.IO + SupervisorJob()),
            produceFile = {
                File(context.filesDir, "datastore/$fileName").also { file ->
                    file.parentFile?.mkdirs()
                }
            }
        )
    }

    private object TestAuthSessionCipher : AuthSessionCipher() {
        const val PREFIX = "encrypted:"

        override fun encrypt(plainText: String): String = PREFIX + plainText

        override fun decrypt(encryptedPayload: String): String {
            return encryptedPayload.removePrefix(PREFIX)
        }
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
