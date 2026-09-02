package com.dc.melodiasmario.shared.di

import com.dc.melodiasmario.core.auth.di.AuthModule
import com.dc.melodiasmario.core.network.di.NetworkModule
import com.dc.melodiasmario.core.settings.di.SettingsModule as CoreSettingsModule
import com.dc.melodiasmario.feature.currentmusic.di.CurrentMusicModule
import com.dc.melodiasmario.feature.guild.di.GuildModule
import com.dc.melodiasmario.feature.login.di.LoginModule
import com.dc.melodiasmario.feature.playlist.di.PlaylistModule
import com.dc.melodiasmario.feature.playlistsong.di.PlaylistSongModule
import com.dc.melodiasmario.feature.profile.di.ProfileModule
import com.dc.melodiasmario.feature.queue.di.QueueModule
import com.dc.melodiasmario.feature.removesong.di.RemoveSongModule
import com.dc.melodiasmario.feature.settings.di.SettingsModule
import org.koin.core.annotation.ComponentScan
import org.koin.core.annotation.Module

@Module(
    includes = [
        NetworkModule::class,
        AuthModule::class,
        PlaylistSongModule::class,
        CurrentMusicModule::class,
        LoginModule::class,
        GuildModule::class,
        PlaylistModule::class,
        ProfileModule::class,
        QueueModule::class,
        RemoveSongModule::class,
        SettingsModule::class,
        CoreSettingsModule::class,
    ],
)
@ComponentScan("com.dc.melodiasmario.shared")
class SharedModule
