package com.dc.melodiasmario.core.domain.currentuser

import com.dc.melodiasmario.core.model.currentuser.CurrentUser

interface CurrentUserRepository {
    suspend fun getCurrentUser(): CurrentUser
}