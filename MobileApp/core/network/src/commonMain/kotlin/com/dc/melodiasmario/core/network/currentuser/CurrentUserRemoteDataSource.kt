package com.dc.melodiasmario.core.network.currentuser

import com.dc.melodiasmario.core.model.currentuser.CurrentUser

interface CurrentUserRemoteDataSource {
    suspend fun getCurrentUser(accessToken: String): CurrentUser
}