package com.dc.melodiasmario.core.data.auth

import com.dc.melodiasmario.core.model.auth.isExpiredOrCloseToExpiry
import com.dc.melodiasmario.core.domain.auth.AuthRepository
import com.dc.melodiasmario.core.domain.auth.SecureAuthSessionStorage
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
