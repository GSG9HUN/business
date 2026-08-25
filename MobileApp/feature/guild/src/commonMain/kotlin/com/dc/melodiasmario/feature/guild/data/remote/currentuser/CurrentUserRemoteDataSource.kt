package com.dc.melodiasmario.feature.guild.data.remote.currentuser

import com.dc.melodiasmario.feature.guild.domain.model.currentuser.CurrentUser

interface CurrentUserRemoteDataSource {
    suspend fun getCurrentUser(accessToken: String): CurrentUser
}