package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import org.koin.core.annotation.Single

@Single
class AuthSessionMemoryCache {
    private val mutex = Mutex()
    private var cachedSession: AuthSession? = null

    suspend fun getOrLoad(loadSession: suspend () -> AuthSession?): AuthSession? {
        return mutex.withLock {
            cachedSession ?: loadSession()?.also { cachedSession = it }
        }
    }

    suspend fun save(session: AuthSession) {
        mutex.withLock {
            cachedSession = session
        }
    }

    suspend fun clear() {
        mutex.withLock {
            cachedSession = null
        }
    }
}
