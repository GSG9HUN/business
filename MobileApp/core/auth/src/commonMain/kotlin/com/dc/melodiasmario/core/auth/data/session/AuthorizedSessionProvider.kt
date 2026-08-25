package com.dc.melodiasmario.core.auth.data.session

import com.dc.melodiasmario.core.auth.domain.model.isExpiredOrCloseToExpiry
import com.dc.melodiasmario.core.auth.domain.repository.AuthRepository
import com.dc.melodiasmario.core.auth.domain.storage.SecureAuthSessionStorage
import org.koin.core.annotation.Single

@Single
class AuthorizedSessionProvider(
    private val secureAuthSessionStorage: SecureAuthSessionStorage,
    private val authRepository: AuthRepository
) {
    suspend fun getValidSession(): String{
        val session = secureAuthSessionStorage.getSession()
            ?: throw Exception("No session found")

        val validSession = if (session.isExpiredOrCloseToExpiry()){
            authRepository.refreshSession(session.refreshToken)
        } else {
            session
        }
        return validSession.accessToken
    }
}
