package com.dc.melodiasmario.shared.data.auth.session

import com.dc.melodiasmario.shared.domain.auth.model.isExpiredOrCloseToExpiry
import com.dc.melodiasmario.shared.domain.auth.repository.AuthRepository
import com.dc.melodiasmario.shared.domain.auth.storage.SecureAuthSessionStorage
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