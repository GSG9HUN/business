package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
import com.dc.melodiasmario.core.model.auth.isExpiredOrCloseToExpiry
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import org.koin.core.annotation.Single

@Single
class AuthorizedSessionProvider(
    private val secureAuthSessionStorage: SecureAuthSessionStorage,
    private val authRepository: AuthRepository,
    private val authSessionMemoryCache: AuthSessionMemoryCache,
) {
    private val refreshMutex = Mutex()

    suspend fun getValidSession(): String {
        return refreshMutex.withLock {
            val session = authSessionMemoryCache.getOrLoad {
                secureAuthSessionStorage.getSession()
            } ?: throw Exception("No session found")

            val validSession = if (session.isExpiredOrCloseToExpiry()) {
                authRepository.refreshSession(session.refreshToken)
            } else {
                session
            }
            authSessionMemoryCache.save(validSession)
            validSession.accessToken
        }
    }
}
