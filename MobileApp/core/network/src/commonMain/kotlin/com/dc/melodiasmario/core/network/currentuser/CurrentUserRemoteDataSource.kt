package com.dc.melodiasmario.core.network.currentuser

import com.dc.melodiasmario.core.model.currentuser.CurrentUser

/**
 * Defines remote current user data operations.
 */
interface CurrentUserRemoteDataSource {
    /**
     * Retrieves the authenticated user account.
     */
    suspend fun getCurrentUser(accessToken: String): CurrentUser
}
