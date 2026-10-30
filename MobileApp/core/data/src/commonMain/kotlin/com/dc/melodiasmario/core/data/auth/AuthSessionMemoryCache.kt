package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.model.auth.AuthSession
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import org.koin.core.annotation.Single

/**
 * Provides auth session memory cache for the auth layer.
 */
@Single
class AuthSessionMemoryCache {
    private val mutex = Mutex()
    private var cachedSession: AuthSession? = null

    /**
     * Returns the cached session, loading it through the supplied callback when necessary.
     */
    suspend fun getOrLoad(loadSession: suspend () -> AuthSession?): AuthSession? {
        return mutex.withLock {
            cachedSession ?: loadSession()?.also { loadedSession -> cachedSession = loadedSession }
        }
    }

    /**
     * Stores the supplied value in auth session memory cache.
     */
    suspend fun save(session: AuthSession): Unit {
        mutex.withLock {
            cachedSession = session
        }
    }

    /**
     * Clears the state and resources owned by AuthSessionMemoryCache.
     */
    suspend fun clear(): Unit {
        mutex.withLock {
            cachedSession = null
        }
    }
}
