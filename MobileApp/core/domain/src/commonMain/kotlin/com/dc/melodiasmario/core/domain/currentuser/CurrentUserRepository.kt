package com.dc.melodiasmario.core.domain.currentuser

import com.dc.melodiasmario.core.model.currentuser.CurrentUser

/**
 * Defines current user data operations independently of their concrete data sources.
 */
interface CurrentUserRepository {
    /**
     * Retrieves the authenticated user account.
     */
    suspend fun getCurrentUser(): CurrentUser
}
