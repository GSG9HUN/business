package com.dc.melodiasmario.shared.ui.navigation

object AppRoute {
    const val Login = "login"
    const val GuildSelector = "guild_selector"

    const val Profile = "profile/{profileId}"
    const val Playlists = "playlists/{guildId}"

    fun profile(profileId: String) = "profile/$profileId"
    fun playlists(guildId: String) = "playlists/$guildId"

    const val PlaylistSongs = "playlist_songs"
    const val Queue = "queue"
    const val CurrentMusic = "current_music"
    const val AddSong = "add_song"
    const val RemoveSong = "remove_song"
    const val Settings = "settings"
}
