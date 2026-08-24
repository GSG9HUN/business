package com.dc.melodiasmario.feature.guild.domain.repository.currentuser

import com.dc.melodiasmario.feature.guild.domain.model.currentuser.CurrentUser

interface CurrentUserRepository {
    suspend fun getCurrentUser(): CurrentUser
}